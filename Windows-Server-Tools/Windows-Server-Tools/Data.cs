﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Server_Tools
{
    public class Data
    {
        public static string SimpsonsUsers = "objectClass,sAMAccountName,dn\r\nuser,Hsimpson,\"CN=Homer Simpson,OU=simpsons,DC=jackson,dc=local\"\r\nuser,msimpson,\"CN=Marge Simpson,OU=simpsons,DC=jackson,DC=local\"\r\nuser,bsimpson,\"CN=Bart Simpson,OU=simpsons,DC=jackson,DC=local\"\r\nuser,lsimpson,\"CN=Lisa Simpson,OU=simpsons,DC=jackson,DC=local\"\r\nuser,masimpson,\"CN=Maggie Simpson,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hakira,\"CN=Happy Akira,OU=simpsons,DC=jackson,DC=local\"\r\nuser,malbright,\"CN=Ms Albright,OU=simpsons,DC=jackson,DC=local\"\r\nuser,aamadopolis,\"CN=Aristotle Amadopolis,OU=simpsons,DC=jackson,DC=local\"\r\nuser,satkins,\"CN=StateComptroller Atkins,OU=simpsons,DC=jackson,DC=local\"\r\nuser,mbailey,\"CN=Mary Bailey,OU=simpsons,DC=jackson,DC=local\"\r\nuser,bbarlow,\"CN=Birchibald Barlow,OU=simpsons,DC=jackson,DC=local\"\r\nuser,jbeardly,\"CN=Jasper Beardly,OU=simpsons,DC=jackson,DC=local\"\r\nuser,dbenjamin,\"CN=Doug Benjamin,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hbmarty,\"CN=Happy Bill and Marty,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hblinky,\"CN=Happy Blinky,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hboobarella,\"CN=Happy Boobarella,OU=simpsons,DC=jackson,DC=local\"\r\nuser,wborton,\"CN=Wendell Borton,OU=simpsons,DC=jackson,DC=local\"\r\nuser,gbouvier,\"CN=Gladys Bouvier,OU=simpsons,DC=jackson,DC=local\"\r\nuser,jbouvier,\"CN=Jacqueline Bouvier,OU=simpsons,DC=jackson,DC=local\"\r\nuser,pbouvier,\"CN=Patty Bouvier,OU=simpsons,DC=jackson,DC=local\"\r\nuser,sbouvier,\"CN=Selma Bouvier,OU=simpsons,DC=jackson,DC=local\"\r\nuser,kbrockman,\"CN=Kent Brockman,OU=simpsons,DC=jackson,DC=local\"\r\nuser,bman,\"CN=Happy Bumblebee Man,OU=simpsons,DC=jackson,DC=local\"\r\nuser,cburns,\"CN=Charles Burns,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hcgoofball,\"CN=Happy Capital City Goofball,OU=simpsons,DC=jackson,DC=local\"\r\nuser,ccarlson,\"CN=Carl Carlson,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hcharlie,\"CN=Happy Charlie,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hchase,\"CN=Happy Chase,OU=simpsons,DC=jackson,DC=local\"\r\nuser,schristian,\"CN=Scott Christian,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hcowboy,\"CN=Happy Cowboy Bob,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hcomicbook,\"CN=Happy Comic Book Guy,OU=simpsons,DC=jackson,DC=local\"\r\nuser,mcostington,\"CN=Mr. Costington,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hdatabase,\"CN=Happy Database,OU=simpsons,DC=jackson,DC=local\"\r\nuser,ddesmond,\"CN=Delcan Desmond,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hdiscostu,\"CN=Happy Disco Stu,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hdolph,\"CN=Happy Dolph,OU=simpsons,DC=jackson,DC=local\"\r\nuser,ldoris,\"CN=Lunchlady Doris,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hduffman,\"CN=Happy Duffman,OU=simpsons,DC=jackson,DC=local\"\r\nuser,egunter,\"CN=Ernst Gunter,OU=simpsons,DC=jackson,DC=local\"\r\nuser,ftony,\"CN=Fat Tony,OU=simpsons,DC=jackson,DC=local\"\r\nuser,mflanders,\"CN=Maude Flanders,OU=simpsons,DC=jackson,DC=local\"\r\nuser,nflanders,\"CN=Ned Flanders,OU=simpsons,DC=jackson,DC=local\"\r\nuser,rflanders,\"CN=Rod Flanders,OU=simpsons,DC=jackson,DC=local\"\r\nuser,tflanders,\"CN=Todd Flanders,OU=simpsons,DC=jackson,DC=local\"\r\nuser,fthesquealer,\"CN=Frankie the Squealer,OU=simpsons,DC=jackson,DC=local\"\r\nuser,pfrink,\"CN=Professor Frink,OU=simpsons,DC=jackson,DC=local\"\r\nuser,bgerald,\"CN=Baby Gerald,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hginger,\"CN=Happy Ginger,OU=simpsons,DC=jackson,DC=local\"\r\nuser,mglick,\"CN=Mrs. Glick,OU=simpsons,DC=jackson,DC=local\"\r\nuser,gloria,\"CN=Happy Gloria,OU=simpsons,DC=jackson,DC=local\"\r\nuser,bgumble,\"CN=Barney Gumble,OU=simpsons,DC=jackson,DC=local\"\r\nuser,ggunderson,\"CN=Gil Gunderson,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hlittleelves,\"CN=Happy Little Elves,OU=simpsons,DC=jackson,DC=local\"\r\nuser,jharm,\"CN=Judge Harm,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hherman,\"CN=Happy Herman,OU=simpsons,DC=jackson,DC=local\"\r\nuser,bhibbert,\"CN=Bernice Hibbert,OU=simpsons,DC=jackson,DC=local\"\r\nuser,drhibbert,\"CN=Dr.Julius Hibbert,OU=simpsons,DC=jackson,DC=local\"\r\nuser,ehoover,\"CN=MissElizabeth Hoover,OU=simpsons,DC=jackson,DC=local\"\r\nuser,lhutz,\"CN=Lionel Hutz,OU=simpsons,DC=jackson,DC=local\"\r\nuser,scratchy,\"CN=Scratchy Itchy,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hjacques,\"CN=Happy Jacques,OU=simpsons,DC=jackson,DC=local\"\r\nuser,jjones,\"CN=Jimbo Jones,OU=simpsons,DC=jackson,DC=local\"\r\nuser,rjordan,\"CN=Rachel Jordan,OU=simpsons,DC=jackson,DC=local\"\r\nuser,happykang,\"CN=Happy Kang and Kodos,OU=simpsons,DC=jackson,DC=local\"\r\nuser,pkashmir,\"CN=Princess Kashmir,OU=simpsons,DC=jackson,DC=local\"\r\nuser,hkearney,\"CN=Happy Kearney,OU=simpsons,DC=jackson,DC=local\"\r\nuser,ekrabappel,\"CN=Edna Krabappel,OU=simpsons,DC=jackson,DC=local\"\r\nuser,rkrusfski,\"CN=RabbiHyman Krustofski,OU=simpsons,DC=jackson,DC=local\"\r\nuser,kclown,\"CN=Krusty The Clown,OU=simpsons,DC=jackson,DC=local\"\r\nuser,ckwan,\"CN=Cookie Kwan,OU=simpsons,DC=jackson,DC=local\"\r\nuser,dlargo,\"CN=Dewey Largo,OU=simpsons,DC=jackson,DC=local\"\r\nuser,rwiggum,\"CN=Ralph Wiggum,OU=simpsons,DC=jackson,DC=local\"\r\nuser,swiggum,\"CN=Sarah Wiggum,OU=simpsons,DC=jackson,DC=local\"\r\nuser,gwillie,\"CN=GroundsKeeper Willie,OU=simpsons,DC=jackson,DC=local\"\r\n";
    }
}