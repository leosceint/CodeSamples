//Sample: displaying contents of Windows Shell special folders (such as My Computer, Control Panel etc.) in WPF WebBrowser
// *** Main Window Codebehind ***
//Copyright (c) 2019, MSDN.WhiteKnight
//(Requires reference to System.Drawing)
//Based on: https://devblogs.microsoft.com/oldnewthing/?p=9773

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Net;

namespace WpfApplication1
{
    public partial class MainWindow : Window
    {
        const int port = 8080;
        const string prefix = "Folder";        
        ShellItemsHttpListener listener=null;                

        /* Shell Special Folders table */
        List<ShellSpecialFolder> folders = new List<ShellSpecialFolder>
        {
    new ShellSpecialFolder("My Computer",new Guid("0AC0837C-BBF8-452A-850D-79D08E667CA7")),
    new ShellSpecialFolder("Network",Guid.Parse("D20BEEC4-5CA8-4905-AE3B-BF251EA09B53")),    
    new ShellSpecialFolder("Control Panel",Guid.Parse("82A74AEB-AEB4-465C-A014-D097EE346D63")),
    new ShellSpecialFolder("Printers",Guid.Parse("76FC4E2D-D6AD-4519-A663-37BD56068185")),    
    new ShellSpecialFolder("Recycle Bin",Guid.Parse("B7534046-3ECB-4C18-BE4E-64CD4CB7D6AC")),
    new ShellSpecialFolder("Connections",Guid.Parse("6F0CD92B-2E97-45D1-88FF-B0D186B8DEDD")),
    new ShellSpecialFolder("Fonts",Guid.Parse("FD228CB7-AE11-4AE3-864C-16F3910AB8FE")),
    new ShellSpecialFolder("Desktop",Guid.Parse("B4BFCC3A-DB2C-424C-B029-7FE99A87C641")),    
    new ShellSpecialFolder("Programs",Guid.Parse("A77F5D77-2E2B-44C3-A6A2-ABA601054A51")),
    new ShellSpecialFolder("Start Menu",Guid.Parse("625B53C3-AB48-4EC1-BA1F-A1EF4146FC19")),
    new ShellSpecialFolder("Recent",Guid.Parse("AE50C081-EBD2-438A-8655-8A092E34987A")),
    new ShellSpecialFolder("SendTo",Guid.Parse("8983036C-27C0-404B-8F08-102D10DCFD74")),
    new ShellSpecialFolder("Documents",Guid.Parse("FDD39AD0-238F-46AF-ADB4-6C85480369C7")),
    new ShellSpecialFolder("Favorites",Guid.Parse("1777F761-68AD-4D8A-87BD-30B759FA33DD")),
    new ShellSpecialFolder("NetHood",Guid.Parse("C5ABBF53-E17F-4121-8900-86626FC2C973")),
    new ShellSpecialFolder("PrintHood",Guid.Parse("9274BD8D-CFD1-41C3-B35E-B13F55A758F4")),
    new ShellSpecialFolder("Templates",Guid.Parse("A63293E8-664E-48DB-A079-DF759E0509F7")),    
    new ShellSpecialFolder("Common Programs",Guid.Parse("0139D44E-6AFE-49F2-8690-3DAFCAE6FFB8")),
    new ShellSpecialFolder("Common StartMenu",Guid.Parse("A4115719-D62E-491D-AA7C-E74B8BE3B067")),
    new ShellSpecialFolder("Public Desktop",Guid.Parse("C4AA340D-F20F-4863-AFEF-F87EF2E6BA25")),
    new ShellSpecialFolder("Program Data",Guid.Parse("62AB5D82-FDC1-4DC3-A9DD-070D1D495D97")),
    new ShellSpecialFolder("Common Templates",Guid.Parse("B94237E7-57AC-4347-9151-B08C6C32D1F7")),
    new ShellSpecialFolder("Public Documents",Guid.Parse("ED4824AF-DCE4-45A8-81E2-FC7965083634")),
    new ShellSpecialFolder("RoamingAppData",Guid.Parse("3EB685DB-65F9-4CF6-A03A-E3EF65729F3D")),
    new ShellSpecialFolder("LocalAppData",Guid.Parse("F1B32785-6FBA-4FCF-9D55-7B8E7F157091")),
    new ShellSpecialFolder("LocalAppDataLow",Guid.Parse("A520A1A4-1780-4FF6-BD18-167343C5AF16")),
    new ShellSpecialFolder("InternetCache",Guid.Parse("352481E8-33BE-4251-BA85-6007CAEDCF9D")),
    new ShellSpecialFolder("Cookies",Guid.Parse("2B0F765D-C0E9-4171-908E-08A611B84FF6")),
    new ShellSpecialFolder("History",Guid.Parse("D9DC8A3B-B784-432E-A781-5A1130A75963")),    
    new ShellSpecialFolder("Profile",Guid.Parse("5E6C858F-0E22-4760-9AFE-EA3317B67173")),
    new ShellSpecialFolder("Pictures",Guid.Parse("33E28130-4E1E-4676-835A-98395C3BC3BB")),    
    new ShellSpecialFolder("AdminTools",Guid.Parse("724EF170-A42D-4FEF-9F26-B60E846FBA4F")),
    new ShellSpecialFolder("CommonAdminTools",Guid.Parse("D0384E7D-BAC3-4797-8F14-CBA229B392B5")),
    new ShellSpecialFolder("Music",Guid.Parse("4BD8D571-6D19-48D3-BE97-422220080E43")),
    new ShellSpecialFolder("Videos",Guid.Parse("18989B1D-99B5-455B-841C-AB7C74E4DDFC")),
    new ShellSpecialFolder("Ringtones",Guid.Parse("C870044B-F49E-4126-A9C3-B52A1FF411E8")),
    new ShellSpecialFolder("Public Pictures",Guid.Parse("B6EBFB86-6907-413C-9AF7-4FC2ABF07CC5")),
    new ShellSpecialFolder("Public Music",Guid.Parse("3214FAB5-9757-4298-BB61-92A9DEAA44FF")),
    new ShellSpecialFolder("Public Videos",Guid.Parse("2400183A-6185-49FB-A2D8-4A392A602BA3")),
    new ShellSpecialFolder("Public Ringtones",Guid.Parse("E555AB60-153B-4D17-9F04-A5FE99FC15EC")),    
    new ShellSpecialFolder("CDBurning",Guid.Parse("9E52AB10-F80D-49DF-ACB8-4330F5687855")),
    new ShellSpecialFolder("User Profiles",Guid.Parse("0762D272-C50A-4BB0-A382-697DCD729B80")),
    new ShellSpecialFolder("Playlists",Guid.Parse("DE92C1C7-837F-4F69-A3BB-86E631204A23")),
    new ShellSpecialFolder("Sample Playlists",Guid.Parse("15CA69B3-30EE-49C1-ACE1-6B5EC372AFB5")),
    new ShellSpecialFolder("Sample Music",Guid.Parse("B250C668-F57D-4EE1-A63C-290EE7D1AA1F")),
    new ShellSpecialFolder("Sample Pictures",Guid.Parse("C4900540-2379-4C75-844B-64E6FAF8716B")),
    new ShellSpecialFolder("Sample Videos",Guid.Parse("859EAD94-2E85-48AD-A71A-0969CB56A6CD")),
    new ShellSpecialFolder("Photo Albums",Guid.Parse("69D2CF90-FC33-4FB7-9A0C-EBB0F0FCB43C")),
    new ShellSpecialFolder("Public",Guid.Parse("DFDF76A2-C82A-4D63-906A-5644AC457385")),
    new ShellSpecialFolder("ChangeRemovePrograms",Guid.Parse("DF7266AC-9274-4867-8D55-3BD661DE872D")),
    new ShellSpecialFolder("AppUpdates",Guid.Parse("A305CE99-F527-492B-8B1A-7E76FA98D6E4")),
    new ShellSpecialFolder("AddNewPrograms",Guid.Parse("DE61D971-5EBC-4F02-A3A9-6C82895E5C04")),
    new ShellSpecialFolder("Downloads",Guid.Parse("374DE290-123F-4565-9164-39C4925E467B")),
    new ShellSpecialFolder("Public Downloads",Guid.Parse("3D644C9B-1FB8-4F30-9B45-F670235F79C0")),
    new ShellSpecialFolder("SavedSearches",Guid.Parse("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA")),
    new ShellSpecialFolder("QuickLaunch",Guid.Parse("52A4F021-7B75-48A9-9F6B-4B87A210BC8F")),
    new ShellSpecialFolder("Contacts",Guid.Parse("56784854-C6CB-462B-8169-88E350ACB882")),
    new ShellSpecialFolder("SidebarParts",Guid.Parse("A75D362E-50FC-4FB7-AC2C-A8BEAA314493")),
    new ShellSpecialFolder("SidebarDefaultParts",Guid.Parse("7B396E54-9EC5-4300-BE0A-2482EBAE1A26")),
    new ShellSpecialFolder("PublicGameTasks",Guid.Parse("DEBF2536-E1A8-4C59-B6A2-414586476AEA")),
    new ShellSpecialFolder("GameTasks",Guid.Parse("054FAE61-4DD8-4787-80B6-090220C4B700")),
    new ShellSpecialFolder("SavedGames",Guid.Parse("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4")),
    new ShellSpecialFolder("Games",Guid.Parse("CAC52C1A-B53D-4EDC-92D7-6B2E8AC19434")),    
    new ShellSpecialFolder("Links",Guid.Parse("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968")),
    new ShellSpecialFolder("UsersFiles",Guid.Parse("F3CE0F7C-4901-4ACC-8648-D5D44B04EF8F")),
    new ShellSpecialFolder("UsersLibraries",Guid.Parse("A302545D-DEFF-464B-ABE8-61C8648D939B")),
    new ShellSpecialFolder("SearchHome",Guid.Parse("190337D1-B8CA-4121-A639-6D472D16972A")),
    new ShellSpecialFolder("OriginalImages",Guid.Parse("2C36C0AA-5812-4B87-BFD0-4CD0DFB19B39")),
    new ShellSpecialFolder("DocumentsLibrary",Guid.Parse("7B0DB17D-9CD2-4A93-9733-46CC89022E7C")),
    new ShellSpecialFolder("MusicLibrary",Guid.Parse("2112AB0A-C86A-4FFE-A368-0DE96E47012E")),
    new ShellSpecialFolder("PicturesLibrary",Guid.Parse("A990AE9F-A03B-4E80-94BC-9912D7504104")),
    new ShellSpecialFolder("VideosLibrary",Guid.Parse("491E922F-5643-4AF4-A7EB-4E7A138D8174")),
    new ShellSpecialFolder("RecordedTVLibrary",Guid.Parse("1A6FDBA2-F42D-4358-A798-B74D745926C5")),
    new ShellSpecialFolder("HomeGroup",Guid.Parse("52528A6B-B9E3-4ADD-B60D-588C2DBA842D")),
    new ShellSpecialFolder("HomeGroupCurrentUser",Guid.Parse("9B74B6A3-0DFD-4f11-9E78-5F7800F2E772")),
    new ShellSpecialFolder("DeviceMetadataStore",Guid.Parse("5CE4A5E9-E4EB-479D-B89F-130C02886155")),
    new ShellSpecialFolder("Libraries",Guid.Parse("1B3EA5DC-B587-4786-B4EF-BD1DC332AEAE")),
    new ShellSpecialFolder("Public Libraries",Guid.Parse("48DAF80B-E6CF-4F4E-B800-0E69D84EE384")),
    new ShellSpecialFolder("UserPinned",Guid.Parse("9E3995AB-1F9C-4F13-B827-48B24B6C7174")),
    new ShellSpecialFolder("ImplicitAppShortcuts",Guid.Parse("BCB5256F-79F6-4CEE-B725-DC34E402FD46")),
    new ShellSpecialFolder("AccountPictures",Guid.Parse("008CA0B1-55B4-4C56-B8A8-4DE4B299D3BE")),
    new ShellSpecialFolder("PublicUserTiles",Guid.Parse("0482AF6C-08F1-4C34-8C90-E17EC98B1E17")),
    new ShellSpecialFolder("AppsFolder",Guid.Parse("1E87508D-89C2-42F0-8A7E-645A0F50CA58")),   
    new ShellSpecialFolder("Application Shortcuts",Guid.Parse("A3918781-E5F2-4890-B3D9-A7E54332328C")),
    new ShellSpecialFolder("RoamingTiles",Guid.Parse("00BCFC5A-ED94-4e48-96A1-3F6217F21990")),
    new ShellSpecialFolder("RoamedTileImages",Guid.Parse("AAA8D5A5-F1D6-4259-BAA8-78E7EF60835E")),
    new ShellSpecialFolder("Screenshots",Guid.Parse("B7BEDE81-DF94-4682-A7D8-57A52620B86F")),
    new ShellSpecialFolder("CameraRoll",Guid.Parse("AB5FB87B-7CE2-4F83-915D-550846C9537B")),
    new ShellSpecialFolder("SkyDrive",Guid.Parse("A52BBA46-E9E1-435F-B3D9-28DAA648C0F6")),
    new ShellSpecialFolder("OneDrive",Guid.Parse("A52BBA46-E9E1-435F-B3D9-28DAA648C0F6")),    
    new ShellSpecialFolder("SearchHistory",Guid.Parse("0D4C3DB6-03A3-462F-A0E6-08924C41B5D4")),
    new ShellSpecialFolder("SearchTemplates",Guid.Parse("7E636BFE-DFA9-4D5E-B456-D7B39851D8A9")),
    new ShellSpecialFolder("CameraRollLibrary",Guid.Parse("2B20DF75-1EDA-4039-8097-38798227D5B7")),
    new ShellSpecialFolder("SavedPictures",Guid.Parse("3B193882-D3AD-4EAB-965A-69829D1FB59F")),
    new ShellSpecialFolder("SavedPicturesLibrary",Guid.Parse("E25B5812-BE88-4BD9-94B0-29233477B6C3")),
    new ShellSpecialFolder("RetailDemo",Guid.Parse("12D4C69E-24AD-4923-BE19-31321C43A767")),
    new ShellSpecialFolder("Device",Guid.Parse("1C2AC1DC-4358-4B6C-9733-AF21156576F0")),    
    new ShellSpecialFolder("3D Objects",Guid.Parse("31C0DD25-9439-4F12-BF41-7FF4EDA38722")),
    new ShellSpecialFolder("AppCaptures",Guid.Parse("EDC0FE71-98D8-4F4A-B920-C8DC133CB165")),
    new ShellSpecialFolder("Local Documents",Guid.Parse("F42EE2D3-909F-4907-8871-4C22FC0BF756")),
    new ShellSpecialFolder("Local Pictures",Guid.Parse("0DDD015D-B06C-45D5-8C4C-F59713854639")),
    new ShellSpecialFolder("Local Videos",Guid.Parse("35286A68-3C57-41A1-BBB1-0EAE73D76C95")),
    new ShellSpecialFolder("Local Music",Guid.Parse("A0C69A99-21C8-4671-8703-7934162FCF1D")),
    new ShellSpecialFolder("Local Downloads",Guid.Parse("7D83EE9B-2244-4E70-B1F5-5393042AF1E4")),    
        };

        public MainWindow()
        {         
            InitializeComponent();
            listbox.ItemsSource = this.folders;
            webbrowser.Navigating += webbrowser_Navigating;
        }

        void webbrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            //обработка перенаправлений URL с http:// на file://
            if (listener != null)
            {
                Uri uri = listener.ProcessRedirect(e.Uri.AbsoluteUri);

                if (uri != null)
                {
                    e.Cancel = true;
                    try { webbrowser.Navigate(uri); }
                    catch (Exception)
                    {
                        string s = Uri.UnescapeDataString(uri.AbsolutePath.Substring(1));
                        System.Diagnostics.Process.Start(s);
                    }                    
                }
            }
        }        

        private void listbox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            object item = listbox.SelectedItem;
            if (item == null) return;
            ShellSpecialFolder folder = (ShellSpecialFolder)item;

            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }

            this.Cursor = System.Windows.Input.Cursors.Wait;
            List<ShellItem> items;
            try
            {
                items = GetItems(folder.Guid); //получаем список элементов каталога 
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
           
            listener = new ShellItemsHttpListener(items, prefix, port);
            listener.Start(); //запускаем HTTP-сервер
            webbrowser.Navigate("http://localhost:" + port.ToString() + "/" + prefix + "/"); //открываем страницу в WebBrowser            
        }

        public static List<ShellItem> GetItems(Guid FolderID) //Получает список элементов каталога по GUID
        {            
            IntPtr p = IntPtr.Zero;
            IShellFolder pFolder = null;
            IEnumIDList pEnum = null;
            IntPtr pItem = IntPtr.Zero;                      
            IntPtr lpStr = IntPtr.Zero;
            STRRET strret;
            Guid guid = typeof(IShellFolder).GUID;
            List<ShellItem> items = new List<ShellItem>();
            ShellItem si;

            try
            {
                int hr = NativeMethods.SHGetKnownFolderIDList(ref FolderID, 0, IntPtr.Zero, out p);
                if (hr != 0) throw Marshal.GetExceptionForHR(hr);

                hr = NativeMethods.SHBindToObject(null, p, null, ref guid, out pFolder);
                if (hr != 0) throw Marshal.GetExceptionForHR(hr);

                pFolder.EnumObjects(IntPtr.Zero, NativeMethods.SHCONTF_FOLDERS | NativeMethods.SHCONTF_NONFOLDERS, out pEnum);

                while (true)
                {
                    pItem = IntPtr.Zero;
                    uint res = pEnum.Next(1, out pItem, IntPtr.Zero);
                    if (res != 0) break;
                    si = new ShellItem();

                    //display name
                    lpStr = IntPtr.Zero;
                    strret = new STRRET();
                    pFolder.GetDisplayNameOf(pItem, NativeMethods.SHGDN_NORMAL, out strret);
                    hr = NativeMethods.StrRetToStr(ref strret, pItem, out lpStr);
                    if (hr != 0) throw Marshal.GetExceptionForHR(hr);
                    string s = Marshal.PtrToStringUni(lpStr);
                    si.DisplayName = s;
                    NativeMethods.CoTaskMemFree(lpStr);

                    //path
                    lpStr = IntPtr.Zero;
                    strret = new STRRET();
                    pFolder.GetDisplayNameOf(pItem, NativeMethods.SHGDN_FORPARSING, out strret);
                    hr = NativeMethods.StrRetToStr(ref strret, pItem, out lpStr);
                    if (hr != 0) throw Marshal.GetExceptionForHR(hr);
                    s = Marshal.PtrToStringUni(lpStr);
                    try { si.Path = new Uri(s); }
                    catch (UriFormatException) { si.Path = new Uri("file://localhost/"+s); }
                    NativeMethods.CoTaskMemFree(lpStr);

                    //icon
                    try
                    {
                        Guid iid_IIExtractIcon = typeof(IExtractIcon).GUID;
                        IExtractIcon pExtract;
                        pFolder.GetUIObjectOf(IntPtr.Zero, 1, new IntPtr[] { pItem }, ref iid_IIExtractIcon, 0, out pExtract);

                        StringBuilder sbIcon = new StringBuilder(260);
                        int index = 0;
                        uint flags;
                        hr = pExtract.GetIconLocation(NativeMethods.GIL_FORSHELL, sbIcon, 260, out index, out flags);
                        if (hr == 0)
                        {
                            IntPtr hIconSmall = IntPtr.Zero, hIconLarge = IntPtr.Zero;
                            hr = pExtract.Extract(sbIcon.ToString(), (uint)index, out hIconLarge, out hIconSmall, 0x00140014);
                            if (hr == 0 && hIconSmall != IntPtr.Zero)
                            {
                                var icon = System.Drawing.Icon.FromHandle(hIconSmall);
                                var bitmap = icon.ToBitmap();

                                using (bitmap)
                                {
                                    MemoryStream ms = new MemoryStream();
                                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                    si.Image = ms.ToArray();
                                }

                                NativeMethods.DestroyIcon(hIconSmall);
                                NativeMethods.DestroyIcon(hIconLarge);
                            }
                            else { si.Image = new byte[0]; }
                        }
                        else
                        {
                            si.Image = new byte[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        si.Image = new byte[0];
                    }
                    items.Add(si);
                    NativeMethods.CoTaskMemFree(pItem);
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.GetType().ToString());
            }
            finally
            {
                if (p != IntPtr.Zero) NativeMethods.CoTaskMemFree(p);
                if (pFolder != null) Marshal.ReleaseComObject(pFolder);
                if (pEnum != null) Marshal.ReleaseComObject(pEnum);
            }
            return items;
        }            
    }

    public struct ShellItem
    {
        public string DisplayName { get; set; }
        public Uri Path { get; set; }
        public byte[] Image { get; set; }
    }

    public struct ShellSpecialFolder
    {
        string _name;
        Guid _guid;
        public string Name { get { return _name; }  }
        public Guid Guid { get { return _guid; } }

        public ShellSpecialFolder(string name, Guid guid)
        {
            _name = name; _guid = guid;
        }
    }       
}
