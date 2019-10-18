# Custom frame in Windows Forms using DWM API

This sample demonstrates using DWM API to customize form non-client area. It uses [DwmExtendFrameIntoClientArea](https://docs.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmextendframeintoclientarea) function to extend frame into client area and adjusts messages handling to make sure mouse clicks are handled properly. Then we can put menu into the window caption. All controls put into former client area must be painted manually to be displayed.

Form Designer look:

[![designer][1]][1]

Runtime look:

[![runtime][2]][2]

Reqiures .NET 4.5+ & Windows Vista or newer (DWM Composition must be enabled on Vista/7).

Based on [Custom Window Frame Using DWM](https://docs.microsoft.com/en-us/windows/win32/dwm/customframe).

[1]: https://i.stack.imgur.com/hRoC0.png
[2]: https://i.stack.imgur.com/g6Ac9.png
