   M S B A M L   `   `   `  ����F  BStreamParty, Version=6.2.4.1, Culture=neutral, PublicKeyToken=null%clr-namespace:FilmParty	FilmParty  5   	       FilmParty.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:FilmParty     5   	   S OMaterialDesignThemes.Wpf, Version=1.0.1.0, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   -   5           MaterialDesignBody#���   5	   	     MaterialDesignPaper#���  5
   	   $��Medium"�5   	   $��14(�5   	     � Title$  Stream Party��5   	   $��490.659��6   $��318.538��6/    � 
ResizeMode$ CanMinimize=�6?    � WindowStartupLocation$ CenterScreen=�6X   ��5   	   :  4MaterialDesignThemes.Wpf.MaterialDesignFontExtension  .��5      � ���	#FF0E1621.+�5   
   ��  $  MaterialDesignHeadline2TextBlock#��[ 6   $	��Top=�6N   $
��Left=�6f   $��	10,10,0,0q�6�   $��35(�6�   
��������� 6�   $�Party Log-In��6�   6�   � 5   
   -   5        $��LogInContainer��5      $��
10,72,10,0q�6%    � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    .+�5      � $
��Auto��6   $��0��6!   $	��Top=�6.    � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $
��Auto �6'   �� 5      $
��Auto �6'    5      �� 5      $
��Auto �6$   .+�5!      �� -   5         ��Name$ 	AdaServer��5!      $��0��6,   $��0��6<   
����������#2f6ea5$��0,0,5,15q�6v   $��0q�6�   +  %MaterialDesignThemes.Wpf.RippleAssist   Feedback �	#FF0E1621  MaterialDesignFlatButton#��[ 6�    � ToolTip$8 2Resource name: MaterialDesignRaisedLightDarkButton��6�   $��40��64  $��90��6@  .��5"      � $
��Auto��6   $
��Auto��6(    5#      �� 5$      $
��Auto �6/   �� 5%      $
��Auto �6/   .+�5'      '  !MaterialDesignThemes.Wpf.PackIcon     Kind$ SpeedometerSlow=�62   $��0��6I   $��Center=�6Y   $��0,0,5,0q�6t   �� 5(      $��Adafruit��6$   $��1��64    ! MaterialDesignButtonTextBlock#��[ 6D   $��Center=�6{   5)      5*      �� 5+      -   5        $ SolaceServer��5+      $��1��6/   $��0��6?   ���#2f6ea5$��0,0,0,15q�6a   $��0q�6s    �	#FF0E1621#��[ 6�   $8 2Resource name: MaterialDesignRaisedLightDarkButton��6�   $��40��6  $��90��6+  .��5,      � $
��Auto��6   $
��Auto��6(    5-      �� 5.      $
��Auto �6/   �� 5/      $
��Auto �6/   .+�51        $ Rocket=�62   $��0��6@   $��Center=�6P   $��0,0,5,0q�6k   �� 52      $��Solace��6$   $��1��62   #��[ 6B   $��Center=�6y   53      54      �� 55      -   5        $ 	LoginFree��55      $��2��6,   $��0��6<   ���#2f6ea5$��5,0,0,15q�6^   $��0q�6p    �	#FF0E1621#��[ 6�   $8 2Resource name: MaterialDesignRaisedLightDarkButton��6�   $��40��6  $��90��6(  .��56      � $
��Auto��6   $
��Auto��6(    57      �� 58      $
��Auto �6/   �� 59      $
��Auto �6/   .+�5;        $ Login=�62   $��0��6?   $��Center=�6O   $��0,0,5,0q�6j   �� 5<      $��Only Me��6$   $��1��63   #��[ 6C   $��Center=�6z   5=      5>      5?      � 5A      -   5        	 �Name$	 AdafruitLoginBox��5A      $
��Auto��6-   $��1��6;   $	��Top=�6H   $y�Visible=�6`    5B      �� 5C      $
��Auto �6$   �� 5D      $
��Auto �6$   �� 5E      $
��Auto �6$   .+�5G      �� -   5        
 ��Name$
 NickName��5G      $��0��6,   $��0,0,0,5q�69   )  #MaterialDesignThemes.Wpf.HintAssist   
Foreground
 ��������#FFFFFF �� 
CaretBrush
 �����.  (MaterialDesignThemes.Wpf.TextFieldAssist   UnderlineBrush
 ����� ( $MaterialDesignFilledTextFieldTextBox#��[ 6�   $	��Top=�6   �� AcceptsReturn .   �� TextWrapping$
 Wrap=�6A  $	��400��6U   �� 	MaxLength$ 25��6d     Hint$ Nickname (anyone)��6s  �� 5H      -   5        $
 UsernameTxt��5H      $��1��6/   $��0,0,0,5q�6<   
 ��������#FFFFFF
 �����
 �����#��[ 6�    .  $
 Wrap=�6,  $	��400��6@  $ Username��6O  $ 40��6y  $	��Top=�6�  G� 5I      -	   5         G�Name$ 	AioKeyTxt��5I      $��2��61   $��0,0,0,20q�6>   
 ��������#FFFFFF G� 
CaretBrush
 �����
 ����� 0 ,MaterialDesignFilledPasswordFieldPasswordBox#��[ 6�   $	��400��6!  $ Aio Key��60  $	��Top=�6Y  5J      � 5K      -
   5        $	 SolaceLoginBox��5K      $
��Auto��6+   $��1��69   $	��Top=�6F   $y�Hidden=�6^    5L      �� 5M      $
��Auto �6$   �� 5N      $
��Auto �6$   �� 5O      $
��Auto �6$   .+�5Q      �� -   5        $
 SolaceNickName��5Q      $��0��62   $��0,0,0,5q�6?   
 ��������#FFFFFF
 �����
 �����#��[ 6�   $	��Top=�6   .  $
 Wrap=�6G  $	��400��6[  $ 25��6j  $ Nickname (anyone)��6y  �� 5R      -   5        $
 SolaceHostPort��5R      $��1��62   $��0,0,0,5q�6?   
 ��������#FFFFFF
 �����
 �����#��[ 6�    .  $ NoWrap=�6/  $ Secured MQTT Host : Port��6E  $	��Top=�6  G� 5S      -   5        $ SolacePassword��5S      $��2��66   $��0,0,0,20q�6C   
 ��������#FFFFFF
 �����
 �����#��[ 6�   $	��400��6&  $ Password��65  $	��Top=�6_  5T      � 5V      $��2��6   $
��Auto��6    $	��Top=�6.    5W      �� 5X      $
��Auto �6$   �� 5Y      $
��Auto �6$   �� 5Z      $
��Auto �6$   .+�5\      �� -   5         ��Name$ RememberCheck��5\   (   $��0��6   ���#2f6ea5$��0,0,0,7q�6T     MaterialDesignCheckBox#��[ 6e   $��Remember data��6�   $
��Left=�6�   $	��Top=�6�   
�������6�   � 5]      $��0��6   $��Right=�6%   $	��Top=�6A   $��0,0,0,7q�6Z    5^      �� 5_      $
��Auto �6+   �� 5`      $
��Auto �6+   .+�5b        $	 Key=�6.   $��0,0,7,0q�69   $��0��6J   ���#2f6ea56Z   �� 5c      $��1��6    �� 61   -   5         ��Name$ PasteKey��5c   ;   ���#2f6ea5 $  MaterialDesignSubtitle2Hyperlink#��[ 6b   .�6�   �� $�I have a key��6�   6�   6�   5d      �� 5e      $��1��6   $	��Top=�6)   $��0,0,0,12q�6A   $
��Left=�6S   �� 6o   -   5        $ Register��5e   y   ���#2f6ea5#��[ 6�   .�6�   �� $�Create account��6�   6  6  �� 5f      $��1��6   $	��Top=�6)   $��Right=�6A   $��0,0,0,12q�6]   �� 6p   -   5        $ Update��5f   z   ���#2f6ea5#��[ 6�   .�6�   �� $�Check update��6�   6
  6  �� 5g      -   5        $ LogInBtn��5g      $��2��6+   ���#2f6ea5$��0,7,0,0q�6M    "	 MaterialDesignRaisedDarkButton#��[	 6^   $��Log-In��6�   $8 2Resource name: MaterialDesignRaisedLightDarkButton��6�   $��40��6�   $��Bottom=�6�   $��Right=�6  $��90��6'  62  �� 5h      -   5        $ CloseBtn��5h      $��2��6+   ���#2f6ea5$��0,7,0,0q�6M   #��[ 6^   $��Close��6�   $ MaterialDesignFlatButton��6�   $��40��6�   $��Bottom=�6�   $
��Left=�6�   $��90��6  6  5i      5k      � 5l   
   -   5         �Name$ ProgressCircle��5l      ���#2f6ea5 %
 !MaterialDesignCircularProgressBar#��[
 6C    � Value$ 0Q�6~    � IsIndeterminate .  $��42��6�   $��42��6�   $��	0,10,10,0q�6�   $	��Top=�6�   $��Right=�6�   '  !MaterialDesignThemes.Wpf.Snackbar  5m   
   -   5          Name$ SnackbarLogIn��5m   3      IsActive .  
�������$��	12,0,13,0q�6�      MessageQueue 6]   4  .MaterialDesignThemes.Wpf.MessageQueueExtension     Message. 5n      .  (MaterialDesignThemes.Wpf.SnackbarMessage  $�� ��6-   ���#212121$��Center=�6M   5o      5p      5q      