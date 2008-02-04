//using System;
//
//namespace WeSay.UI
//{
//    internal class Keyman
//    {
//        private readonly KeymanImpl keyman;
//        public Keyman ()
//	    {
//            keyman = null;
//            if (Environment.OSVersion.Platform != PlatformID.Unix)
//            {
//                try
//                {
//                    keyman = new KeymanImpl();
//                }
//                catch
//	            {
//	            }
//            }
//	    }
//
//        public void UseKeyboard(string keyboard)
//        {
//            if(keyman != null)
//            {
//                keyman.UseKeyboard(keyboard);
//            }
//        }
//
//        public void ClearKeyboard()
//        {
//            if(keyman != null)
//            {
//                keyman.ClearKeyboard();
//            }
//        }
//    }
//
//    internal class KeymanImpl
//    {
//        private readonly KeymanLink.KeymanLink _keyman6;
//        private readonly Keyman7Adaptor _keyman7;
//
//        public KeymanImpl()
//        {
//            try
//            {
//                _keyman6 = new KeymanLink.KeymanLink();
//                if (_keyman6.Initialize(false))
//                {
//                    return; // we're good with Keyman 6 support
//                }
//                _keyman6 = null;
//            }
//            catch (Exception)
//            {
//                //swallow.  we have a report from Mike that indicates the above will
//                //crash in some situation (vista + keyman 6.2?)... better to just not
//                // provide direct keyman access in that situation
//                _keyman6 = null;
//            }
//
//
//            try
//            {
//                _keyman7 = new Keyman7();
//            }
//            catch (Exception)
//            {
//                _keyman7 = null;
//            }
//        }
//
//        public void UseKeyboard(string keyboard)
//        {
//            if (string.IsNullOrEmpty(keyboard))
//            {
//                return;
//            }
//
//            try
//            {
//                if (_keyman6 != null)
//                {
//                    _keyman6.SelectKeymanKeyboard(keyboard, true);
//                }
//                else
//                    if (_keyman7 != null)
//                    {
//                        _keyman7.UseKeyboard(keyboard);
//                    }
//
//            }
//            catch (Exception err)
//            {
//                Palaso.Reporting.ErrorReport.ReportNonFatalMessage("Keyman switching problem: " + err.Message);
//            }
//        }
//
//        public void ClearKeyboard()
//        {
//            if (this._keyman6 != null)
//            {
//                this._keyman6.SelectKeymanKeyboard(null, false);
//            }
//            else if (this._keyman7 != null)
//            {
//                _keyman7.ClearKeyboard();
//            }
//        }
//    }
//}
