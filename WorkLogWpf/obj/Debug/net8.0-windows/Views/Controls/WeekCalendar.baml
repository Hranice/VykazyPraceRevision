   M S B A M L   `   `   `  ����E  AWorkLogWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:WorkLogWpf.Views.ControlsWorkLogWpf.Views.Controls  5      ,    &WorkLogWpf.Views.Controls.WeekCalendar   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      9local'clr-namespace:WorkLogWpf.Views.Controls     5      
�������.��5      �   � ColumnDefinitions  5   
   �� 5      $	��100 �6   �� 5	      $��* �6   .+�5   
   � -   5        $��DayLabelGrid��5      $��0��6    � RowDefinitions 5      �� 5      $��40 �6    �� 5      $��* �6    �� 5      $��* �6    �� 5      $��* �6    �� 5      $��* �6    �� 5      $��* �6    �� 5      $��* �6    �� 5      $��* �6    .+�5      �� $��0��6   �� 5      $��1��6   $��Pondělí
7.4.2025��6%   $��Center=�6B   $��5q�6]   �� 5      $��2��6   $��Úterý
8.4.2025��6%   $��Center=�6@   $��5q�6[   �� 5      $��3��6   $��Středa
9.4.2025��6%   $��Center=�6A   $��5q�6\   �� 5      $��4��6   $��Čtvrtek
10.4.2025��6%   $��Center=�6C   $��5q�6^   �� 5      $��5��6   $��Pátek
11.4.2025��6%   $��Center=�6A   $��5q�6\   �� 5      $��6��6   $��Sobota
12.4.2025��6%   $��Center=�6B   $��5q�6]   �� 5       $��7��6   $��Neděle
13.4.2025��6%   $��Center=�6B   $��5q�6]   5!      �� 5$   
   $��1��6   $
��Auto=�6'   $��Disabled=�6L   .��5%      � -   5        $��CalendarGrid��5%      
�������$��0q�5(       � 	Resources 5)       �� 
TargetType   GridCellBorderStyle �� Property �� Value��%~   &	        �� 5+      " ��6   .�5,      ��  � �� �#EEE68   �� 5-       � ��$ 0,0,1,1q�6<   5.      5/       51      �� 52      $��40 �6$   �� 53      $��* �6$   �� 54      $��* �6$   �� 55      $��* �6$   �� 56      $��* �6$   �� 57      $��* �6$   �� 58      $��* �6$   �� 59      $��* �6$   5=      5>      5?      5@      