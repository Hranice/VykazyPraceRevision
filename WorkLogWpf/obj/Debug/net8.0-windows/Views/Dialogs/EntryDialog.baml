   M S B A M L   `   `   `  ����  
WorkLogWpf*    $WorkLogWpf.Views.Dialogs.EntryDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Upravit záznam��5   	   $	��600��6    $	��400��6-    � WindowStartupLocation$ CenterOwner=�69   .��5      �� $��20q�6   $	��Top=�6   .+�5   
   � $��0,0,0,10q�6   $��Center=�6&   .+�5	      d� $��PROVOZ��6   $��4q�6S    d� 	IsChecked 6,   �� *IsCategoryProvoz+6S   d� 5
      $��PROJEKT��6   $��4q�6U    6-   �� *IsCategoryProjekt+6U   d� 5      $��	ŠKOLENÍ��6   $��4q�6U    6-   �� *IsCategorySkoleni+6U   d� 5      $��NEPŘÍTOMNOST��6   $��4q�6_    62   �� *IsCategoryNepritomnost+6_   d� 5      $��OSTATNÍ��6   $��4q�6U    6-   �� *IsCategoryOstatni+6U   5      �� 5   
   $��Nákladové středisko*��6   �� 5   
    �� DisplayMemberPath$ ProjectTitle��5      $��0,4,0,10q�5      ��5      �� *AvailableProjects+ �� SelectedItem 5      �� *SelectedProject+5      �� 5   
   $��Typ záznamu*��6   � 5   
   $��0,4,0,10q�6   .+�5      d� $��Práce��6   $��4q�6R    6+   �� *IsEntryTypePrace+6R   d� 5      $��Administrativa��6   $��4q�6d    64   �� *IsEntryTypeAdministrativa+6d   d� 5      $��Meeting��6   $��4q�6V    6-   �� *IsEntryTypeMeeting+6V   5      �� 5    
   $��Index*��6   �� 5!   
   $ Title��5#      $��0,4,0,10q�5$      ��5!      �� *AvailableSubTypes+ 5"      �� *SelectedSubType+5$      � 5'   
   $��0,0,0,10q�6   .+�5(      �� $	��100��6   $��0,0,8,0q�6%   .+�5)      �� $��
Počátek*��6   �� 5*      $��0,4,0,0q�5,      ��5*      �� *AvailableStartTimes+ 5+      �� *SelectedStartTime+5,      5-      �� 5.      $	��100��6   .+�5/      �� $��Ukončení*��6   �� 50      $��0,4,0,0q�52      ��50      �� *AvailableEndTimes+ 51      �� *SelectedEndTime+52      53      54      �� 57   
   $��	Poznámka��6   �� 58   
   $��80��6(   $��0,4,0,10q�64    �� TextWrapping$
 Wrap=�6F    �� AcceptsReturn$
 True��6Z   ��6   �� *Note+6Z   �� 5;   
    �� Orientation$ 
Horizontal=�6   $��Right=�6.   .+�5<      �� $��	Odstranit��6   $	��100��6)   $��4q�65   ��6@   �� *DeleteCommand+�� 5=      $��Potvrdit��6   $	��100��6(   $��4q�64   ��6?   �� *ConfirmCommand+5>      5@      5A      