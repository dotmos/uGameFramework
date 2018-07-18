using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.MemoryBrowserService {

    partial class MemoryBrowserServiceImpl : MemoryBrowserServiceBase {

        ReactiveDictionary<string, MemoryBrowser> browsers = new ReactiveDictionary<string, MemoryBrowser>();

        protected override void AfterInitialize() {

        }

        public override MemoryBrowser CreateMemoryBrowser(string id,object root) {
            var mBrowser = new MemoryBrowser(root);
            browsers.Add(id, mBrowser);
            return mBrowser;
        }

        public override MemoryBrowser GetBrowser(string id) {
            if (browsers.ContainsKey(id)) {
                return browsers[id];
            } else {
                UnityEngine.Debug.LogError("Tried to access unknown MemoryBrowser with id:" + id);
                return null;
            }
        }

        public override bool IsSimpleType(object obj) {
            return MemoryBrowser.IsSimple(obj.GetType());
        }

        public override ReactiveDictionary<string, MemoryBrowser> rxGetAllBrowsers() {
            return browsers;
        }

        protected override void OnDispose() {
            foreach (var browser in browsers) {
                browser.Value.Dispose();
            }
            browsers.Dispose();
            browsers.Clear();
            browsers = null;
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

    }



}
