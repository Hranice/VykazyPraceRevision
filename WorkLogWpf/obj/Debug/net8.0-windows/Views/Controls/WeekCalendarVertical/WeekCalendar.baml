   M S B A M L   `   `   `  ����E  AWorkLogWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullo<clr-namespace:WorkLogWpf.Views.Controls.WeekCalendarVertical.WorkLogWpf.Views.Controls.WeekCalendarVertical  5      A    ;WorkLogWpf.Views.Controls.WeekCalendarVertical.WeekCalendar   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Nlocal<clr-namespace:WorkLogWpf.Views.Controls.WeekCalendarVertical     5      
�������.��5      �   � RowDefinitions  5   
   �� 5	      $
��Auto �6   �� 5
      $��* �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��Center=�6;   $
��0,10q�6X   .+�5      �� -   5        $��← Předchozí týden��5      $��5q�61   $��5q�6<   6H   �� 5      -   5         ��Name$ 	WeekLabel��5      $��Center=�6+   $
��Bold"�6F   $��14(�6X   $
��10,0q�6f   �� 5      -   5        $��Následující týden →��5      $��5q�63   $��5q�6>   6J   5      � 5   
   $��1��6     5      �� 5      $
��Auto �6    �� 5      $��* �6    .+�5      � -   5        $��DayLabelGrid��5       $��0��6    � ColumnDefinitions 5      �� 5      $��40 �6'   �� 5      $��* �6'   �� 5       $��* �6'   �� 5!      $��* �6'   �� 5"      $��* �6'   �� 5#      $��* �6'   �� 5$      $��* �6'   �� 5%      $��* �6'   �� 5&      $
��Auto �6'   .+�5)      �� $��0��6   �� 5*      $��1��6   $��	Pondělí��6,   $��Center=�6;   $��5q�6X   $
��Wrap=�6c   �� 5+      $��2��6   $��Úterý��6,   $��Center=�69   $��5q�6V   $
��Wrap=�6a   �� 5,      $��3��6   $��Středa��6,   $��Center=�6:   $��5q�6W   $
��Wrap=�6b   �� 5-      $��4��6   $��Čtvrtek��6,   $��Center=�6;   $��5q�6X   $
��Wrap=�6c   �� 5.      $��5��6   $��Pátek��6,   $��Center=�69   $��5q�6V   $
��Wrap=�6a   �� 5/      $��6��6   $��Sobota��6,   $��Center=�6:   $��5q�6W   $
��Wrap=�6b   �� 50      $��7��6   $��Neděle��6,   $��Center=�6:   $��5q�6W   $
��Wrap=�6b   51      �� 54      $��1��6   $
��Auto=�6(   $��Disabled=�6K   .��55      � -   5        $��CalendarGrid��55      
�������$��0q�58       � 	Resources 59       �� 
TargetType   GridCellBorderStyle �� Property �� Value��%~   &	        �� 5:      " ��6    .�5;      ��  � �� �#EEE6<   �� 5<       � ��$ 1,1,0,0q�6@   5=      5>       5@      �� 5A      $��40 �6+   �� 5B      $��* �6+   �� 5C      $��* �6+   �� 5D      $��* �6+   �� 5E      $��* �6+   �� 5F      $��* �6+   �� 5G      $��* �6+   �� 5H      $��* �6+   5J      5K      5L      5M      5N      