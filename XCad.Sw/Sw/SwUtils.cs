using System;
using System.Diagnostics;
using SolidWorks.Interop.sldworks;

namespace XCad.Sw {
    public static class SwUtils {
        // 1. 使用 Lazy 确保线程安全和延迟加载
        private static Lazy<IMathUtility> _math = CreateMathLazy();
        private static Lazy<IModeler> _modeler = CreateModelerLazy();
        private static SwApplication _app;

        // 定义如何创建 Lazy 实例的工厂方法
        private static Lazy<IMathUtility> CreateMathLazy() => new Lazy<IMathUtility>(() => Sw?.IGetMathUtility());
        private static Lazy<IModeler> CreateModelerLazy() => new Lazy<IModeler>(() => Sw?.IGetModeler());

        public static IMathUtility Math => _math.Value;
        public static IModeler Modeler => _modeler.Value;

        // 直接获取当前的 SldWorks 实例
        public static SldWorks Sw => (SldWorks)App.Sw;

        internal static SwApplication App {
            get {
                if(_app == null) {
                    // 注意：FromProcess 在多实例环境下可能抓错进程，建议由外部 Add-in 入口传入实例
                    _app = (SwApplication)SwApplicationFactory.FromProcess(Process.GetCurrentProcess());
                }
                return _app;
            }
            set => _app = value;
        }

        /// <summary>
        /// 关键方法：当插件卸载（Disconnect）或发生异常需要重置时调用
        /// </summary>
        public static void Release() {
            _math = CreateMathLazy();
            _modeler = CreateModelerLazy();
            _app = null;
            // 强制回收一次，确保旧的 COM 引用被释放
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}