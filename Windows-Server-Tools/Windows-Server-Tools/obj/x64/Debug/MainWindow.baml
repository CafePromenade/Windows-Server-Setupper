   M S B A M L   `   `   `  ����O  KWindows-Server-Tools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:Windows_Server_ToolsWindows_Server_Tools  5   	   %    Windows_Server_Tools.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:Windows_Server_Tools     5   	     � Title$  
MainWindow��5   	    � SizeToContent$ WidthAndHeight=�6M   .��5	      � .+�5
   
   �� -   5         ��Name$" InstallActiveDirectoryButton��5
      $��Submit��67   $
��Left=�6H   $��49��6c   $��
23,125,0,0q�6o   $	��Top=�6�   $	��226��6�   6�   �� 5   
   -   5         ��Name$ DomainNameTextBox��5      $
��Left=�6-   $��	27,64,0,0q�6H    �� TextWrapping$
 Wrap=�6[   $	��Top=�6o   $	��220��6�   $��42��6�   �� 5   
   $
��Left=�6   $��	32,10,0,0q�6/   $
��Wrap=�6B   $��Install Active Directory��6V   $	��Top=�6v   $��20(�6�   $��Bahnschrift)�6�   �� 5   
   $
��Left=�6   $��	27,43,0,0q�6/   $
��Wrap=�6B   $)��#Please enter a new domain name here��6V   $	��Top=�6�   �� 5   
   -   5        $��Set Static IP��5      $
��Left=�6)   $��
277,10,0,0q�6D   $	��Top=�6X   $��30��6p   $	��158��6|   6�   �� 5   
   $
��Left=�6   $��170,192,0,0q�6/   $
��Wrap=�6D   $��Simpsons Users��6X   $	��Top=�6n   $��20(�6�   $��Bahnschrift)�6�   �� 5   
   -   5        $9��3Import, Spread and Create Groups and Shared Folders��5      $
��Left=�6O   $��32��6j   $��
27,221,0,0q�6v   $	��Top=�6�   $	��412��6�   6�   � 5   
   $
��Left=�6   $	��353��6*   $��
456,10,0,0q�67   $	��Top=�6K   $	��282��6c   ���	#FFD3EAC9.+�5      �� $��Center=�6   $��0,2,0,0q�65   $
��Wrap=�6F   $��Software Installation��6Z   $	��Top=�6w   $��20(�6�   $��Bahnschrift)�6�   �� 5      -   5        $ InstallChocolateyButton��5      $��Install Chocolatey��66   $
��Left=�6S   $��29��6n   $��	13,33,0,0q�6z   $	��Top=�6�   $	��246��6�   6�   �� 5      -   5        $% InstallChocolateySoftwareButton��5      $��Start installing software��6>   $
��Left=�6b   $��27��6}   $��
13,140,0,0q�6�   $	��Top=�6�   $	��246��6�   6�   �� 5      -   5        $ ChocoSoftwareTextBox��5      $
 Wrap=�64   $��13,85,23,186q�6H   �� 5      $
��Left=�6   $��	13,64,0,0q�63   $
��Wrap=�6F   $.��(Enter software you would like to install��6Z   $	��Top=�6�   �� 5      $
��Left=�6   $��
72,172,0,0q�63   $
��Wrap=�6G   $	��Top=�6[   $��20(�6s   $��Bahnschrift)�6�   �� 6�    � Language$ en-ca�6�   $�Omega Server��6�   6�   �� 5      -   5        $ OmegaServerPromoteButton��5      $��Promote to Omega Server��67   $
��Left=�6Y   $��37��6t   $��
13,201,0,0q�6�   $	��Top=�6�   $	��155��6�   {�.  6�   �� 5      -	   5        $ 
SCCMButton��5      $��	SCCM Only��6)   $
��Left=�6=   $��37��6X   $��173,201,0,0q�6d   $	��Top=�6y   $��99��6�   {�.  6�   �� 5      -
   5        $ SideServerButton��5      {�.  $��Install Side Server��6A   $
��Left=�6_   $��35��6z   $��
13,243,0,0q�6�   $	��Top=�6�   $	��259��6�   5      � 5   
   $
��Left=�6   $��1��6*   $��
23,270,0,0q�65   $	��Top=�6I   $��9��6a   � 5   
   -   5         �Name$ InstallServerStuffGrid��5      $��Center=�6/   $	��152��6L   $��	0,368,0,0q�6Y   $	��Top=�6l   $	��729��6�   ���	#FFB4E6E7.+�5      �� $��Center=�6   $��0,5,0,0q�65   $
��Wrap=�6F   $��Install Server Stuff��6Z   $	��Top=�6v   $��20(�6�   $��Bahnschrift)�6�   �� 5      -   5        $ CommonInstallStuffButton��5      $��Open commonly stuff menu��67   $
��Left=�6Z   $��28��6u   $��9,31,0,0q�6�   $	��Top=�6�   $	��169��6�   6�   5       �� 5!   
   -   5        $ AdapterSettingsButton��5!      $��Network Adapter Settings��60   $
��Left=�6S   $��26��6n   $��
277,59,0,0q�6z   $	��Top=�6�   $	��162��6�   6�   5#      5$      