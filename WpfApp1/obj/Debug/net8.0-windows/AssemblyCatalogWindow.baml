   M S B A M L   `   `   `  ����B  >WpfApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:MyApp.WPF	MyApp.WPF  5   	   %    MyApp.WPF.AssemblyCatalogWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   )localclr-namespace:MyApp.WPF     5   	     � Title$!  Каталог сборок��5   	   $	��700��6    $
��1000��6-   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $��* �6   �� 5
      $
��Auto �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��0��6.   $��0,0,0,10q�6;   .+�5      �� $8��2Сортировать по рейтингу (↑)��6   $��0,0,10,0q�6c   ��6;   �� *SortByRatingCommand+6c   �� 5      $8��2Сортировать по рейтингу (↓)��6   $��0,0,10,0q�6g   ��6;   �� *SortByRatingDescCommand+6g   5      �� 5   
   $��1��66   ��6   �� *
Assemblies+ � 	Resources 5       �� 
TargetType �� Property �� Value��%}   '��       �� 5      " ��6   .�5      ��  � ��
 �����64   �� 5       � ��$ 0,0,0,1q�68   5      5       �� View 5      �� 5      .P�5      �� $��Описание��6%    �� Width$	 300��6d   	 �� DisplayMemberBinding	 67   �� *Description+6d   �� 5      $��Оценка��6%   $ 80��6]   	 65   �� *Rating+6]   �� 5      $��Пользователь��6%   $	 150��6;   	 5    %   �� *
User.Email+�� 5!      $#��Общая стоимость��6%   $	 120��6>   	 5"      �� 5#      
 �� Path$
 
TotalPrice�6&    �� StringFormat$ {0:C}��68   5%      �� 5&      $��Компоненты��6%   $	 250��69    �� CellTemplate 5'      �� 5(      .R�5)   "   �� ��6/   �� *AssemblyComponents+��5*   &   �� 5+   *   .R�5,   .   �� ��68   �� *Component.Name+5-   +   5/   #   50      52      �� 53      $��Действия��6%   $	 200��67    54      �� 55      .R�56   "   �� $ 
Horizontal=�6-   .+�57   &   �� $
��👍��6-   $��0,0,5,0q�58   U   ��57   :   �� *!DataContext.LikeAssemblyCommand+ �� RelativeSource ��  �� AncestorType ListView��58   -   �� *
AssemblyID+6U   �� 59   &   $
��👎��6-   $��0,0,5,0q�5:   U   ��59   :   �� *$"DataContext.DislikeAssemblyCommand+ ��  ListView��5:   -   �� *
AssemblyID+6U   �� 5;   &   $��Отзывы��6-   $��0,0,5,0q�5<   U   ��5;   >   �� * DataContext.ShowReviewsCommand+ ��  ListView��5<   -   �� *
AssemblyID+6U   �� 5=   &   $��Удалить��6-   $��0,0,5,0q�5>   U   ��5=   ?   �� *#!DataContext.DeleteAssemblyCommand+ ��  ListView��5>   -   �� *
AssemblyID+y�5?   -   �� *DataContext.IsAdmin+ ��  ListView   BoolToVisibilityConverter �� 	Converter# [  5@   #   5A      5C      5D      5F      �� 5I   
   $��Закрыть��6   $��2��6#   $��Right=�60   $��Bottom=�6L   $��10q�5J      $	��100��6   $��30��6)   ��65   �� *CloseCommand+5K      5L      