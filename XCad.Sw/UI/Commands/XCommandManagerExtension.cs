//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using System.Linq;
using XCad.Sw;
using XCad.Sw.Base.Attributes;
using XCad.UI.Commands.Attributes;
using XCad.UI.Commands.Enums;
using XCad.UI.Commands.Structures;
using XCad.UI.Exceptions;
using XCad.UI.Structures;

namespace XCad.UI.Commands {
    /// <summary>
    /// Additional methods for <see cref="IXCommandManager"/>
    /// </summary>
    public static class XCommandManagerExtension {
        //TODO: think of a way to call Dispose on all wrapped enum groups

        /// <summary>
        /// Specific command spec associated with enumeration
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public class EnumCommandSpec : CommandSpec {
            /// <summary>
            /// Enumeration value of this command spec
            /// </summary>
            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            public Enum Value { get; }

            internal EnumCommandSpec(Enum value, int userId) : base(userId) {
                Value = value;
            }
        }

        internal class EnumCommandSpec<TEnum> : EnumCommandSpec
            where TEnum : Enum {
            internal new TEnum Value => (TEnum)base.Value;

            internal EnumCommandSpec(TEnum value, int userId) : base(value, userId) {
            }
        }

        /// <summary>
        /// Adds command group based on the enumeration where each enumeration field represents the command button
        /// </summary>
        /// <typeparam name="TCmdEnum">Enumeration with commands</typeparam>
        /// <param name="cmdMgr">Command manager</param>
        /// <returns>Created command group</returns>
        /// <remarks>Decorate enumeration and fields with <see cref="TitleAttribute"/>, <see cref="IconAttribute"/>, <see cref="DescriptionAttribute"/>, <see cref="CommandItemInfoAttribute"/> to customized look and feel of commands</remarks>
        public static IEnumCommandGroup<TCmdEnum> AddCommandGroup<TCmdEnum>(this IXCommandManager cmdMgr)
            where TCmdEnum : Enum {
            var id = GetEnumCommandGroupId(cmdMgr, typeof(TCmdEnum), out string tabName);

            var enumGrp = new EnumCommandGroupSpec(typeof(TCmdEnum), id);

            FillEnumCommandGroup<TCmdEnum>(enumGrp, GetEnumCommandGroupParent(cmdMgr, typeof(TCmdEnum)), tabName, id);

            var cmdGrp = cmdMgr.AddCommandGroup(enumGrp);

            return new EnumCommandGroup<TCmdEnum>(cmdGrp);
        }

        /// <summary>
        /// Adds context menu based on the enumeration
        /// </summary>
        ///<inheritdoc cref="AddCommandGroup{TCmdEnum}(IXCommandManager)"/>
        public static IEnumCommandGroup<TCmdEnum> AddContextMenu<TCmdEnum>(this IXCommandManager cmdMgr)
            where TCmdEnum : Enum {
            Type ownerType = typeof(TCmdEnum).TryGetAttribute<ContextMenuCommandGroupInfoAttribute>()?.Owner;

            return AddContextMenu<TCmdEnum>(cmdMgr, ownerType);
        }

        /// <typeparam name="TOwner">Type of the owner where to attach the context menu to</typeparam>
        ///<inheritdoc cref="AddContextMenu{TCmdEnum}(IXCommandManager)"/>
        public static IEnumCommandGroup<TCmdEnum> AddContextMenu<TCmdEnum, TOwner>(this IXCommandManager cmdMgr)
            where TCmdEnum : Enum
            where TOwner : ISwSelObject
            => AddContextMenu<TCmdEnum>(cmdMgr, typeof(TOwner));

        private static IEnumCommandGroup<TCmdEnum> AddContextMenu<TCmdEnum>(IXCommandManager cmdMgr, Type ownerType)
            where TCmdEnum : Enum {
            var id = GetEnumCommandGroupId(cmdMgr, typeof(TCmdEnum), out string tabName);

            var enumGrp = new ContextMenuEnumCommandGroupSpec(typeof(TCmdEnum), id) {
                Owner = ownerType
            };

            FillEnumCommandGroup<TCmdEnum>(enumGrp, GetEnumCommandGroupParent(cmdMgr, typeof(TCmdEnum)), tabName, id);

            var cmdGrp = cmdMgr.AddContextMenu(enumGrp);

            return new EnumCommandGroup<TCmdEnum>(cmdGrp);
        }

        /// <param name="id">Id or -1 to automatically assign</param>
        private static void FillEnumCommandGroup<TCmdEnum>(CommandGroupSpec bar, CommandGroupSpec parent,
            string tabName, int id)
            where TCmdEnum : Enum {
            var cmdGroupType = typeof(TCmdEnum);

            if(parent != null) {
                if(parent.Id == id) {
                    throw new ParentGroupCircularDependencyException($"{parent.Title} ({parent.Id})");
                }
            }

            bar.RibbonTabName = tabName;
            bar.Parent = parent;

            bar.InitFromEnum<TCmdEnum>();

            bar.Commands = Enum.GetValues(cmdGroupType).Cast<TCmdEnum>().Select(
                c => {
                    var enumCmdUserId = Convert.ToInt32(c);

                    if(enumCmdUserId < 0) {
                        enumCmdUserId = 0;//default id (not used)
                    } else {
                        //NOTE: adding one to the id as 0 id means not used while enums start with 0
                        enumCmdUserId++;
                    }

                    return CreateEnumCommand(c, enumCmdUserId);
                }).ToArray();
        }

        private static CommandGroupSpec GetEnumCommandGroupParent(IXCommandManager cmdMgr, Type cmdGroupType) {
            CommandGroupSpec parent = null;

            var grpParentAtt = cmdGroupType.TryGetAttribute<CommandGroupParentAttribute>();

            if(grpParentAtt != null) {
                var groups = cmdMgr.CommandGroups.Select(c => c.Spec);

                if(grpParentAtt.ParentGroupType != null) {
                    var parentGrpSpec = groups.OfType<EnumCommandGroupSpec>()
                        .FirstOrDefault(g => g.CmdGrpEnumType == grpParentAtt.ParentGroupType);
                    if(grpParentAtt.ParentGroupType == cmdGroupType) {
                        throw new ParentGroupCircularDependencyException(grpParentAtt.ParentGroupType.FullName);
                    }

                    parent = parentGrpSpec
                        ?? throw new ParentGroupNotFoundException(grpParentAtt.ParentGroupType.FullName, cmdGroupType.FullName);
                } else {
                    var parentGrpSpec = groups.OfType<CommandGroupSpec>()
                        .FirstOrDefault(g => g.Id == grpParentAtt.ParentGroupUserId)
                        ?? throw new ParentGroupNotFoundException(grpParentAtt.ParentGroupUserId.ToString(), cmdGroupType.FullName);
                    parent = parentGrpSpec;
                }
            }

            return parent;
        }

        private static int GetEnumCommandGroupId(IXCommandManager cmdMgr, Type cmdGroupType, out string tabName) {
            if(!TryGetUserAssignedGroupId(cmdGroupType, out tabName, out int id)) {
                var nextGroupId = 1;

                if(cmdMgr.CommandGroups.Any()) {
                    var usedIds = cmdMgr.CommandGroups.Select(g => g.Spec.Id).OrderBy(x => x).ToArray();

                    for(int i = 0; i < cmdMgr.CommandGroups.Length; i++) {
                        if(usedIds[i] != nextGroupId) {
                            break;
                        }

                        nextGroupId++;
                    }
                }

                id = nextGroupId;
            }

            return id;
        }

        private static bool TryGetUserAssignedGroupId(Type cmdGroupType, out string tabName, out int userId) {
            CommandGroupInfoAttribute grpInfoAtt = cmdGroupType.TryGetAttribute<CommandGroupInfoAttribute>();

            if(grpInfoAtt != null && grpInfoAtt.UserId != -1) {
                userId = grpInfoAtt.UserId;
                tabName = grpInfoAtt.TabName;
                return true;
            }

            userId = -1;
            tabName = "";
            return false;
        }

        private static EnumCommandSpec<TCmdEnum> CreateEnumCommand<TCmdEnum>(TCmdEnum cmdEnum, int userId)
            where TCmdEnum : Enum {
            var cmd = new EnumCommandSpec<TCmdEnum>(cmdEnum, userId);
            var att = cmdEnum.TryGetAttribute<CommandItemInfoAttribute>();

            if(att != null) {
                cmd.HasMenu = att.HasMenu;
                cmd.HasToolbar = att.HasToolbar;
                cmd.SupportedWorkspace = att.SupportedWorkspaces;
                cmd.HasRibbon = att.ShowInCommandTabBox;
                cmd.RibbonTextStyle = att.CommandTabBoxDisplayStyle;
            } else {
                cmd.HasMenu = true;
                cmd.HasToolbar = true;
                cmd.SupportedWorkspace = WorkspaceTypes_e.All;
                cmd.HasRibbon = true;
                cmd.RibbonTextStyle = RibbonTabTextDisplay_e.TextBelow;
            }

            cmd.HasSpacer = cmdEnum.TryGetAttribute<CommandSpacerAttribute>() != null;

            cmd.InitFromEnum(cmdEnum);

            return cmd;
        }
    }
}