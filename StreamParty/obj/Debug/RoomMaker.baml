   M S B A M L   `   `   `  ����F  BStreamParty, Version=6.2.4.1, Culture=neutral, PublicKeyToken=null%clr-namespace:FilmParty	FilmParty  5   	       FilmParty.RoomMaker   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:FilmParty     5   	   S OMaterialDesignThemes.Wpf, Version=1.0.1.0, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	      MaterialDesignBody#���   5	   	     MaterialDesignPaper#���  5
   	   $��Medium"�5   	   $��14(�5   	     � Title$  
Share Room��5   	   $��614.6��6   $��413.631��6+    � 
ResizeMode$ NoResize=�6;    � WindowStartupLocation$ CenterScreen=�6Q   ��5   	   :  4MaterialDesignThemes.Wpf.MaterialDesignFontExtension  .��5      � ���#0E1621.+�5   
   �� -   5        $
��Text��5      $m��gHere you can share this movie room, enter the required data and then send the Key_Room to your friends.��6    ���#FFFFFF$
��Wrap=�6�    $  MaterialDesignHeadline2TextBlock#��[ 6�    �� TextAlignment$
 Left=�6�   $��16(�6  $��54��6  $	��Top=�6!  $��	10,15,0,0q�69  $
��Left=�6L  $	��382��6g  �� 5   
   -   5         ��Name$ CheckCurrentData��5      ���#2f6ea5 d� 	IsChecked$
 TrueY�6B   ���#FFFFFF  MaterialDesignCheckBox#��[ 6h   $��Use current data��6�   $��	10,74,0,0q�6�   $��25��6�   $	��Top=�6�   $
��Left=�6	  $	��144��6$  �� 5   
   -   5        $ CheckControll��5      ���#2f6ea5$ FalseY�6?   ���#FFFFFF#��[ 6f   $��Only I have control��6�   $��
154,74,0,0q�6�   $��25��6�   $	��Top=�6�   $
��Left=�6�   �� 5   
   -   5         ��Name$ NameRoom��5      {�. )  #MaterialDesignThemes.Wpf.HintAssist   
Foreground
 ��������#FFFFFF �� 
CaretBrush
 �����.  (MaterialDesignThemes.Wpf.TextFieldAssist	   UnderlineBrush
	 ����� ( $MaterialDesignFilledTextFieldTextBox#��[ 6�   $	��Top=�6�   
 �� AcceptsReturn
 .  $	��400��6,     Hint$ 	Room Name��6;  $��10,110,10,0q�6f  �� 5   
   -   5        $ DescriptionTxt��5      {�. 
 �����
	 �����
 ��������#FFFFFF#��[ 6�   $	��Top=�6  
 .  $	��400��62  $ Description (optional)��6A  $��10,176,10,0q�6y  � 5   
   -   5        $��CurrentData��5      {�.  $��10,242,10,77q�64   .+�5      �� -   5        $ UsernameTxt��5      
 ��������#FFFFFF#��[ 6m   $	��Top=�6�   
 .   �� TextWrapping$
 Wrap=�6�   $	��400��6�   $ Username��6�   G� 5      -   5         G�Name$ 	AioKeyTxt��5      
 ����� G� 
CaretBrush
 �����
	 ��������#FFFFFF 0 ,MaterialDesignFilledPasswordFieldPasswordBox#��[ 6�   $	��Top=�6�   $	��400��6  $ Aio Key��6%  $��0,66,0,0q�6N  5      �� 5   
   -	   5        $ Key_EndToEnd��5      {�.  
 �����
	 �����
 ��������#FFFFFF#��[ 6�   $	��Top=�6  
 .  $ NoWrap=�61  $	��400��6G  $ Key End-To-End��6V  $��10,373,72,0q�6�  �� 5   
   -
   5         ��Name$ BtnRegenerate��5      
�������  MaterialDesignFlatButton#��[ 6;    � ToolTip$ MaterialDesignFlatButton��6m   $��
0,373,10,0q�6�   $��61��6�   $	��Top=�6  $��Right=�60  $��57��6L  .��5      '  !MaterialDesignThemes.Wpf.PackIcon     Kind$ Restore=�6&   $��29��65   $��27��6A   5      �� 5   
   -   5        $ UrlTxt��5      {�. 
 �����
	 �����
 ��������#FFFFFF#��[ 6�   $	��Top=�6�   
 .  $	��400��6*  $  Link Stream-Url (optional)��69  $��10,439,10,0q�6u  �� 5   
   -   5        $ GenerateBtn��5      ���#2f6ea5 " MaterialDesignRaisedDarkButton#��[ 6;   $"��Generate Key_Group and enter��6s   $8 2Resource name: MaterialDesignRaisedLightDarkButton��6�   $��50��6�   $��Bottom=�6�   $��
10,0,10,10q�6�   6  '  !MaterialDesignThemes.Wpf.Snackbar  5    
   -   5          Name$ SnackbarRoom��5    3      IsActive .  
�������$��	12,0,13,0q�6�      MessageQueue 6\   4  .MaterialDesignThemes.Wpf.MessageQueueExtension     Message. 5!      .  (MaterialDesignThemes.Wpf.SnackbarMessage  $�� ��6-   ���#212121$��Center=�6M   5"      5#      5$      