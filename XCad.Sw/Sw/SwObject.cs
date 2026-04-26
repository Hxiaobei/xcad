using System;
using System.IO;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using XCad.kit.Data;
using XCad.Sw.Base;
using XCad.Sw.Data;
using XCad.Sw.Documents;
using XCad.Sw.Exceptions;

namespace XCad.Sw {
    /// <summary>
    /// Represents base interface for all SOLIDWORKS objects
    /// </summary>
    public interface ISwObject : IEquatable<ISwObject> {
        /// <summary>
        /// Application which owns this object
        /// </summary>
        ISwApplication OwnerApplication { get; }

        /// <summary>
        /// Document which owns this object
        /// </summary>
        /// <remarks>This can be null for the application level objects</remarks>
        ISwDocument OwnerDocument { get; }

        /// <summary>
        /// Identifies if current object is valid
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Provides an ability to store temp tags in this session
        /// </summary>
        ITagsManager Tags { get; }

        /// <summary>
        /// Saves this object into a stream
        /// </summary>
        /// <param name="stream">Target stream</param>
        void Serialize(Stream stream);

        /// <summary>
        /// SOLIDWORKS specific dispatch
        /// </summary>
        object Dispatch { get; }
    }

    /// <inheritdoc/>
    internal class SwObject : ISwObject {
        ISwApplication ISwObject.OwnerApplication => OwnerApplication;
        ISwDocument ISwObject.OwnerDocument => OwnerDocument;

        protected IModelDoc2 OwnerModelDoc => OwnerDocument.Model;

        internal SwApplication OwnerApplication { get; }
        internal virtual SwDocument OwnerDocument { get; }

        public virtual object Dispatch { get; }

        public virtual bool IsAlive {
            get {
                if(Dispatch == null) return false;

                try {
                    // 方案 A：针对几何实体，尝试调用最轻量的底层方法
                    // 如果对象已死，访问任何 COM 属性都会抛出异常
                    if(Dispatch is IEntity entity) return entity.GetType() > 0;
                    

                    // 方案 B：针对特征
                    if(Dispatch is IFeature feat) return !string.IsNullOrEmpty(feat.Name);
                    

                    // 方案 C：通用兜底（访问 IDispatch 的某个基础调用）
                    // 注意：不要使用 GetPersistReference3，除非你真的需要序列化它
                    return Marshal.IsComObject(Dispatch);
                } catch(COMException ex) {
                    // 捕获特定的“对象已失效”异常
                    // 0x80010108: RPC_E_DISCONNECTED (对象已在服务端销毁)
                    return false;
                } catch {
                    return false;
                }
            }
        }

        public ITagsManager Tags => m_TagsLazy.Value;

        private readonly Lazy<ITagsManager> m_TagsLazy;

        internal SwObject(object disp, SwDocument doc, SwApplication app) {
            Dispatch = disp;
            m_TagsLazy = new Lazy<ITagsManager>(() => new GlobalTagsManager(this, app.TagsRegistry));
            OwnerDocument = doc;
            OwnerApplication = app;
        }

        public virtual bool Equals(ISwObject other) {

            if(object.ReferenceEquals(this, other)) return true;

            if(!(other is ISwObject)) return false;

            if(this is IXTransaction ts && !ts.IsCommitted) return false;

            if(other is IXTransaction ts2 && !ts2.IsCommitted) return false;

            if(Dispatch == other.Dispatch) return true;

            return OwnerApplication.Sw.IsSame(Dispatch, other.Dispatch) == (int)swObjectEquality.swObjectSame;
        }

        public virtual void Serialize(Stream stream) {
            if(OwnerModelDoc == null)
                throw new ObjectSerializationException("Model is not set for this object", -1);

            var disp = GetSerializationDispatch() ?? throw new ObjectSerializationException("Dispatch is null", -1);

            // 获取持久引用：这是 SolidWorks 对象的“身份证”
            if(!(OwnerModelDoc.Extension.GetPersistReference3(disp) is byte[] persRef) || persRef.Length == 0)
                throw new ObjectSerializationException("Failed to generate persist reference", -1);

            // 【核心改进】：先存长度，再存内容，保证流的可读性
            byte[] lengthHeader = BitConverter.GetBytes(persRef.Length);
            stream.Write(lengthHeader, 0, lengthHeader.Length);
            stream.Write(persRef, 0, persRef.Length);
        }

        /// <summary>
        /// In some instances it is required to serialize different dispatch (e.g. specific or base feature)
        /// </summary>
        /// <returns></returns>
        protected virtual object GetSerializationDispatch() => Dispatch;

        internal bool CheckIsAlive(Action checker) {
            try {
                checker.Invoke();
                return true;
            } catch {
                return false;
            }
        }
    }
}