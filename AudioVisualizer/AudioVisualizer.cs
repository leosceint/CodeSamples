//Code sample: Sound visualizer using WPF and Core Audio API. 
//Author: MSDN-WhiteKnight (https://github.com/MSDN-WhiteKnight)
//License: BSD 3-clause

//Based on:
//https://ru.stackoverflow.com/questions/586898/c-управление-уровнем-звука-приложения
//https://stackoverflow.com/a/14367829/8674428

//*Main window code-behind*

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Timers;
    
namespace WpfApplication1
{
        public partial class MainWindow : Window
        {
            IMMDeviceEnumerator deviceEnumerator=null;
            IMMDevice speakers = null; //текущее аудиоустройство
            Timer timer; //таймер для обновления UI
            uint this_pid; //идентификатор этого процесса
    
            public MainWindow()
            {
                InitializeComponent();
    
                System.Diagnostics.Process pr = System.Diagnostics.Process.GetCurrentProcess();
                using (pr)
                {
                    this_pid = (uint)pr.Id;
                }
    
                // get default audio device
                deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);
    
                //start UI updating timer
                timer = new Timer(100);
                timer.Elapsed += timer_Elapsed;
                timer.Enabled = true;            
            }
    
            private void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.UpdateVisualizer();
                });
            }
    
            //считывает текущее значение уровней звука и обновляет UI
            public void UpdateVisualizer()
            {
                if (speakers == null) return;
    
                IAudioSessionManager2 mgr = null;
                IAudioSessionEnumerator sessionEnumerator = null;
                IAudioSessionControl ctl = null;
                IAudioSessionControl2 ctl2 = null;
                IAudioMeterInformation meter = null;
    
                try
                {
    
                    // activate the session manager. we need the enumerator
                    Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                    object o;
                    speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                    mgr = (IAudioSessionManager2)o;
    
                    // enumerate sessions for on this device            
                    mgr.GetSessionEnumerator(out sessionEnumerator);
                    int count;
                    sessionEnumerator.GetCount(out count);
    
                    float max_val = 0.0f; //максимальное значение уровня звука для всех сессий
                    int h_min = 50, h_max = 120;//макс. и мин. значение высоты для эллипса
    
                    int hr;    
                    uint pid = 0;
                    float val = 0.0f;                
                    
                    for (int i = 0; i < count; i++)
                    {
                        if (ctl != null) { Marshal.ReleaseComObject(ctl); ctl = null; }
                        if (ctl2 != null) { Marshal.ReleaseComObject(ctl2); ctl2 = null; }
                        if (meter != null) { Marshal.ReleaseComObject(meter); meter = null; }
                                            
                        //получаем WASAPI-сессию
                        hr = sessionEnumerator.GetSession(i, out ctl);
                        if (hr != 0) continue;
    
                        ctl2 = (IAudioSessionControl2)ctl;
                        pid = 0;
                        ctl2.GetProcessId(out pid);
                        if (pid != this_pid) continue; //интересуют только сессии текущего процесса
    
                        meter = (IAudioMeterInformation)ctl;
                        hr = meter.GetPeakValue(out val);//получаем уровень звука
                        if (hr != 0) { continue; }
                        if (val > max_val) max_val = val;                    
    
                    }
    
                    //изменяем высоту эллипса в соответствии со значением максимального уровня звука
                    ellVisualizer.Height = h_min + max_val * (h_max - h_min);
    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), ex.GetType().ToString());
                }
                finally
                {                
                    //очистка ресурсов
                    if (sessionEnumerator != null) { Marshal.ReleaseComObject(sessionEnumerator); sessionEnumerator = null; }
                    if (mgr != null) { Marshal.ReleaseComObject(mgr); mgr = null; }
    
                    if (ctl != null) { Marshal.ReleaseComObject(ctl); ctl = null; }
                    if (ctl2 != null) { Marshal.ReleaseComObject(ctl2); ctl2 = null; }
                    if (meter != null) { Marshal.ReleaseComObject(meter); meter = null; }
                }
    
                
            }
    
            private void Button_Click_1(object sender, RoutedEventArgs e)
            {
                media1.Play();
            }
    
            private void bStop_Click(object sender, RoutedEventArgs e)
            {
                media1.Stop();
            }
    
            private void bOpen_Click(object sender, RoutedEventArgs e)
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.DefaultExt = "mp3";
                ofd.Filter = "Audio files (WAV,MP3,WMA)|*.wav;*.mp3;*.wma|All files|*.*";
                var res = ofd.ShowDialog(this);
                if (res.HasValue)
                {
                    if (res.Value != false)
                    {
                        media1.Source = new Uri(ofd.FileName);
                    }
                }
            }
        }
    
        // *** COM Objects declarations ***
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class MMDeviceEnumerator
        {
        }
    
        internal enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
            EDataFlow_enum_count
        }
    
        internal enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
            ERole_enum_count
        }
    
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            int NotImpl1();
    
            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
    
            // the rest is not implemented
        }
    
        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    
            // the rest is not implemented
        }
    
        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionManager2
        {
            int NotImpl1();
            int NotImpl2();
    
            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
    
            // the rest is not implemented
        }
    
        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int SessionCount);
    
            [PreserveSig]
            int GetSession(int SessionCount, out IAudioSessionControl Session);
        }
    
        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl
        {
            int NotImpl1();
    
            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
    
            // the rest is not implemented
        }
    
        //Источник: https://github.com/maindefine/volumecontrol/blob/master/C%23/CoreAudioApi/Interfaces/IAudioSessionControl2.cs
            [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IAudioSessionControl2
        {
            //IAudioSession functions
            [PreserveSig]
            int GetState(out object state);
            [PreserveSig]
            int GetDisplayName(out IntPtr name);
            [PreserveSig]
            int SetDisplayName(string value, Guid EventContext);
            [PreserveSig]
            int GetIconPath(out IntPtr Path);
            [PreserveSig]
            int SetIconPath(string Value, Guid EventContext);
            [PreserveSig]
            int GetGroupingParam(out Guid GroupingParam);
            [PreserveSig]
            int SetGroupingParam(Guid Override, Guid Eventcontext);
            [PreserveSig]
            int RegisterAudioSessionNotification(object NewNotifications);
            [PreserveSig]
            int UnregisterAudioSessionNotification(object NewNotifications);
            //IAudioSession2 functions
            [PreserveSig]
            int GetSessionIdentifier( out IntPtr retVal);
            [PreserveSig]
            int GetSessionInstanceIdentifier( out IntPtr retVal);
            [PreserveSig]
            int GetProcessId( out UInt32 retvVal);
            [PreserveSig]
            int IsSystemSoundsSession();
            [PreserveSig]
            int SetDuckingPreference( bool optOut);
    
    
        }
    
        //Источник: https://github.com/maindefine/volumecontrol/blob/master/C%23/CoreAudioApi/Interfaces/IAudioMeterInformation.cs
        [Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioMeterInformation 
        {
            [PreserveSig]
            int GetPeakValue(out float pfPeak);
            [PreserveSig]
            int GetMeteringChannelCount(out int pnChannelCount);
            [PreserveSig]
            int GetChannelsPeakValues( int u32ChannelCount,[In]   IntPtr afPeakValues);
            [PreserveSig]
            int QueryHardwareSupport( out int pdwHardwareSupportMask);
        };
}


