   M S B A M L   `   `   `  ����N  JMessenger.WinClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:MessengerWinClientMessengerWinClient  5   	   I)clr-namespace:MessengerWinClient.ControlsMessengerWinClient.Controls  5   	   #    MessengerWinClient.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:MessengerWinClient     5   	   :ctrl)clr-namespace:MessengerWinClient.Controls     5   	   -   5          � Title$  	Messenger��5
   	   $	��450��6   $	��800��6(   .��5      �  � RowDefinitions 5   
   �� 5      6   �� 5      6   �� 5      $
��60px �6   6,    � ColumnDefinitions 5   
   �� 5      $��280px �6   6/   �� 5      6!   �� 5      $��140px �6   6/   .+�5   
   �� -   5        $��0��5      $��1��6$   $��2��64   $��2��6E   
�������$
��Auto=�6s   $��Disabled=�6�   .��5      �� -   5         ��Name$ MessagesStackPanel��5      $
��0,10q�65   {�.  6C   5      �� 5   
   -   5         ��Name$ DialogListView��5      $��Stretch=�5      {�.  $��0��5      $��0��6    $��2��60   $��Disabled=�5       $
��Auto=�5!      �� 5$   
   $��2��6   $��1��6$   $
��Auto=�64   .��5%      �� -   5         ��Name$ MessageText��5%      
�������$	��5pxq�6B   {�.  6P   5&      �� 5(   
   -   5         ��Name$ AddDialogButton��5(      $��2��6*   $��0��67   $��
Add Dialog��6G   $��16(�6\   $��-1,0,1,0q�6�   {�.  6�   �� 5)   
   -   5        $ 
SendButton��5)      $��2��6%   $��2��62   $
��Send��6B   $��16(�6Q   {�.  6{   5*      5+      