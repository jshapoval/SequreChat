   M S B A M L   `   `   `  ���� N  JMessenger.WinClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:MessengerWinClientMessengerWinClient  I)clr-namespace:MessengerWinClient.ControlsMessengerWinClient.Controls  #    MessengerWinClient.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     2local clr-namespace:MessengerWinClient     :ctrl)clr-namespace:MessengerWinClient.Controls     -     � Title$  	Messenger��$	��450��$	��800��.���  � RowDefinitions �� �� �� $
��60px � � ColumnDefinitions �� $��280px ��� �� $��140px �.+��� -   $��0��$��1��$��2��$��2��
�������$
��Auto=�$��Disabled=�.���� -    ��Name$ MessagesStackPanel��$
��0,10q�{�.  �� -    ��Name$ DialogListView��$��Stretch=�{�.  $��0��$��0��$��2��$��Disabled=�$
��Auto=��� $��2��$��1��$
��Auto=�.���� -    ��Name$ MessageText��
�������$	��5pxq�{�.  �� -    ��Name$ AddDialogButton��$��2��$��0��$��
Add Dialog��$��16(�$��-1,0,1,0q�{�.  �� -   $ 
SendButton��$��2��$��2��$
��Send��$��16(�{�.  