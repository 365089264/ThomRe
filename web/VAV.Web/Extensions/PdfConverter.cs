using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

namespace VAV.Web.Extensions
{
    public class PdfConverter
    {
        #region GhostScript Import
        /// <summary>Create a new instance of Ghostscript. This instance is passed to most other gsapi functions. The caller_handle will be provided to callback functions.  
        ///  At this stage, Ghostscript supports only one instance. </summary>  
        /// <param name="pinstance"></param>  
        /// <param name="caller_handle"></param>  
        /// <returns></returns>  
        [DllImport("gsdll64.dll", EntryPoint = "gsapi_new_instance")]
        private static extern int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle);
        /// <summary>This is the important function that will perform the conversion</summary>  
        /// <param name="instance"></param>  
        /// <param name="argc"></param>  
        /// <param name="argv"></param>  
        /// <returns></returns>  
        [DllImport("gsdll64.dll", EntryPoint = "gsapi_init_with_args")]
        private static extern int gsapi_init_with_args(IntPtr instance, int argc, IntPtr argv);
        /// <summary>  
        /// Exit the interpreter. This must be called on shutdown if gsapi_init_with_args() has been called, and just before gsapi_delete_instance().   
        /// </summary>  
        /// <param name="instance"></param>  
        /// <returns></returns>  
        [DllImport("gsdll64.dll", EntryPoint = "gsapi_exit")]
        private static extern int gsapi_exit(IntPtr instance);
        /// <summary>  
        /// Destroy an instance of Ghostscript. Before you call this, Ghostscript must have finished. If Ghostscript has been initialised, you must call gsapi_exit before gsapi_delete_instance.   
        /// </summary>  
        /// <param name="instance"></param>  
        [DllImport("gsdll64.dll", EntryPoint = "gsapi_delete_instance")]
        private static extern void gsapi_delete_instance(IntPtr instance);
        #endregion
        #region Variables
        private string _sDeviceFormat;
        private int _iWidth;
        private int _iHeight;
        private int _iResolutionX;
        private int _iResolutionY;
        private int _iJPEGQuality;
        private Boolean _bFitPage;
        private IntPtr _objHandle;
        #endregion
        #region Proprieties
        public string OutputFormat
        {
            get { return _sDeviceFormat; }
            set { _sDeviceFormat = value; }
        }
        public int Width
        {
            get { return _iWidth; }
            set { _iWidth = value; }
        }
        public int Height
        {
            get { return _iHeight; }
            set { _iHeight = value; }
        }
        public int ResolutionX
        {
            get { return _iResolutionX; }
            set { _iResolutionX = value; }
        }
        public int ResolutionY
        {
            get { return _iResolutionY; }
            set { _iResolutionY = value; }
        }
        public Boolean FitPage
        {
            get { return _bFitPage; }
            set { _bFitPage = value; }
        }
        /// <summary>Quality of compression of JPG</summary>  
        public int JPEGQuality
        {
            get { return _iJPEGQuality; }
            set { _iJPEGQuality = value; }
        }
        #endregion
        #region Init
        public PdfConverter(IntPtr objHandle)
        {
            _objHandle = objHandle;
        }
        public PdfConverter()
        {
            _objHandle = IntPtr.Zero;
        }
        #endregion
        private byte[] StringToAnsiZ(string str)
        {
            //' Convert a Unicode string to a null terminated Ansi string for Ghostscript.  
            //' The result is stored in a byte array. Later you will need to convert 
            //' this byte array to a pointer with GCHandle.Alloc(XXXX, GCHandleType.Pinned)  
            //' and GSHandle.AddrOfPinnedObject()  
            int intElementCount;
            int intCounter;
            byte[] aAnsi;
            byte bChar;
            intElementCount = str.Length;
            aAnsi = new byte[intElementCount + 1];
            for (intCounter = 0; intCounter < intElementCount; intCounter++)
            {
                bChar = (byte)str[intCounter];
                aAnsi[intCounter] = bChar;
            }
            aAnsi[intElementCount] = 0;
            return aAnsi;
        }
        /// <summary>Convert the file!</summary>  
        public void Convert(string inputFile,
            int firstPage, int lastPage, string deviceFormat, int width, int height, int documentID, string picPath)
        {
            //Avoid to work when the file doesn't exist  
            if (!System.IO.File.Exists(inputFile))
            {
                throw new Exception(string.Format("The file :'{0}' doesn't exist", inputFile));
            }
            int intReturn;
            IntPtr intGSInstanceHandle;
            object[] aAnsiArgs;
            IntPtr[] aPtrArgs;
            GCHandle[] aGCHandle;
            int intCounter;
            int intElementCount;
            IntPtr callerHandle;
            GCHandle gchandleArgs;
            IntPtr intptrArgs;
            string[] sArgs = GetGeneratedArgs(inputFile,
                firstPage, lastPage, deviceFormat, width, height, documentID, picPath);
            // Convert the Unicode strings to null terminated ANSI byte arrays  
            // then get pointers to the byte arrays.
            string s = string.Join(" ", sArgs);

            intElementCount = sArgs.Length;
            aAnsiArgs = new object[intElementCount];
            aPtrArgs = new IntPtr[intElementCount];
            aGCHandle = new GCHandle[intElementCount];

            // Create a handle for each of the arguments after   
            // they've been converted to an ANSI null terminated  
            // string. Then store the pointers for each of the handles  
            for (intCounter = 0; intCounter < intElementCount; intCounter++)
            {
                aAnsiArgs[intCounter] = StringToAnsiZ(sArgs[intCounter]);
                aGCHandle[intCounter] = GCHandle.Alloc(aAnsiArgs[intCounter], GCHandleType.Pinned);
                aPtrArgs[intCounter] = aGCHandle[intCounter].AddrOfPinnedObject();
            }
            // Get a new handle for the array of argument pointers  
            gchandleArgs = GCHandle.Alloc(aPtrArgs, GCHandleType.Pinned);
            intptrArgs = gchandleArgs.AddrOfPinnedObject();
            intReturn = gsapi_new_instance(out intGSInstanceHandle, _objHandle);
            callerHandle = IntPtr.Zero;
            try
            {
                intReturn = gsapi_init_with_args(intGSInstanceHandle, intElementCount, intptrArgs);

            }
            catch (Exception )
            {

            }
            finally
            {
                for (intCounter = 0; intCounter < intReturn; intCounter++)
                {
                    aGCHandle[intCounter].Free();
                }
                gchandleArgs.Free();
                gsapi_exit(intGSInstanceHandle);
                gsapi_delete_instance(intGSInstanceHandle);
            }

        }
        private string[] GetGeneratedArgs(string inputFile,
            int firstPage, int lastPage, string deviceFormat, int width, int height, int documentID, string picPath)
        {
            this._sDeviceFormat = deviceFormat;
            this._iResolutionX = width;
            this._iResolutionY = height;
            // Count how many extra args are need - HRangel - 11/29/2006, 3:13:43 PM 
            ArrayList lstExtraArgs = new ArrayList();
            if (_sDeviceFormat == "jpg" && _iJPEGQuality > 0 && _iJPEGQuality < 101)
                lstExtraArgs.Add("-dJPEGQ=" + _iJPEGQuality);
            if (_iWidth > 0 && _iHeight > 0)
                lstExtraArgs.Add("-g" + _iWidth + "x" + _iHeight);
            if (_bFitPage)
                lstExtraArgs.Add("-dPDFFitPage");
            if (_iResolutionX > 0)
            {
                if (_iResolutionY > 0)
                    lstExtraArgs.Add("-r" + _iResolutionX + "x" + _iResolutionY);
                else
                    lstExtraArgs.Add("-r" + _iResolutionX);
            }
            // Load Fixed Args - HRangel - 11/29/2006, 3:34:02 PM  
            int iFixedCount = 18;
            int iExtraArgsCount = lstExtraArgs.Count;
            string[] args = new string[iFixedCount + lstExtraArgs.Count];
            args[0] = "gs";//this parameter have little real use  
            args[1] = "-dSAFER";//I don't want interruptions  
            args[2] = "-dBATCH";//stop after   
            args[3] = "-dNOPAUSE";
            args[4] = "-sDEVICE=" + _sDeviceFormat;//what kind of export format i should provide  
            args[5] = "-r300";
            args[6] = "-dQUIET";
            args[7] = "-dNOPROMPT";
            args[8] = "-dMaxBitmap=500000000";
            args[9] = String.Format("-dFirstPage={0}", firstPage);
            args[10] = String.Format("-dLastPage={0}", lastPage);
            args[11] = "-dAlignToPixels=0";
            args[12] = "-dGridFitTT=0";
            args[13] = "-dTextAlphaBits=4";
            args[14] = "-dGraphicsAlphaBits=4";
            args[15] = "-sPAPERSIZE=a4";
            //For a complete list watch here:  
            //http://pages.cs.wisc.edu/~ghost/doc/cvs/Devices.htm  
            //Fill the remaining parameters  
            for (int i = 0; i < iExtraArgsCount; i++)
            {
                args[16 + i] = (string)lstExtraArgs[i];
            }
            //Fill outputfile and inputfile  
            string fileName = Path.GetFileName(inputFile).Substring(0, Path.GetFileName(inputFile).IndexOf("."));
            args[16 + iExtraArgsCount] = string.Format("-sOutputFile={0}", Path.Combine(picPath, fileName + "%d.jpg "));
            args[17 + iExtraArgsCount] = string.Format("{0}", inputFile);
            return args;
        }
        private int GetPdfPageCount(string pdfName)
        {
            using (FileStream fs = new FileStream(pdfName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader r = new StreamReader(fs))
                {
                    string pdfText = r.ReadToEnd();
                    //count the number of "/Type /Page" .

                    Regex rx1 = new Regex(@"/Type\s*/Page[^s]");
                    MatchCollection matches = rx1.Matches(pdfText);
                    return matches.Count;
                }
            }
        }
        public void pdf2jpg(string pdfName, int documentID, string picPath)
        {
            int pageNo = GetPdfPageCount(pdfName);
            int n = (pageNo > 20 || pageNo == 0) ? 20 : pageNo;
            //this.Convert(pdfName, 1, n, "jpeg", 71, 74, documentID, picPath);
            this.Convert(pdfName, 1, n, "jpeg", 0, 0, documentID, picPath);
        }
    }
}
