   M S B A M L   `   `   `  ���� E  AWorkLogWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullo<clr-namespace:WorkLogWpf.Views.Controls.WeekCalendarVertical.WorkLogWpf.Views.Controls.WeekCalendarVertical  A    ;WorkLogWpf.Views.Controls.WeekCalendarVertical.WeekCalendar   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Nlocal<clr-namespace:WorkLogWpf.Views.Controls.WeekCalendarVertical     
�������.���   � RowDefinitions  �� $
��Auto ��� $��* �.+��� $��0�� �� Orientation$ 
Horizontal=�$��Center=�$
��0,10q�.+��� -   $��← Předchozí týden��$��5q�$��5q��� -    ��Name$ 	WeekLabel��$��Center=�$
��Bold"�$��14(�$
��10,0q��� -   $��Následující týden →��$��5q�$��5q�� $��1��  �� $
��Auto ��� $��* �.+�� -   $��DayLabelGrid��$��0�� � ColumnDefinitions �� $��40 ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $
��Auto �.+��� $��0���� $��1��$��	Pondělí��$��Center=�$��5q�$
��Wrap=��� $��2��$��Úterý��$��Center=�$��5q�$
��Wrap=��� $��3��$��Středa��$��Center=�$��5q�$
��Wrap=��� $��4��$��Čtvrtek��$��Center=�$��5q�$
��Wrap=��� $��5��$��Pátek��$��Center=�$��5q�$
��Wrap=��� $��6��$��Sobota��$��Center=�$��5q�$
��Wrap=��� $��7��$��Neděle��$��Center=�$��5q�$
��Wrap=��� $��1��$
��Auto=�$��Disabled=�.��� -   $��CalendarGrid��
�������$��0q� � 	Resources  �� 
TargetType   GridCellBorderStyle �� Property �� Value��%K   &	        �� " ��.���  � �� �#EEE��  � ��$ 1,1,0,0q� �� $��40 ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* �