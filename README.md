
This Solution contains Sitecore Item **Reference Updater** which will updates references of given item and its childrens(n level), with respect to given source(s) and destination(s).

Projects : 
-----------

1. Sitecore.SharedModule.ReferenceUpdater : Main Reference Updater logic.
2. Sitecore.SharedModule.ReferenceUpdater.Foundry : Events and Processor for Sitecore Foundry
3. Sitecore.SharedModule.ReferenceUpdater.CMS : Events and Processor for Sitecore CMS
4. Website : Contains config file in include folder for Foundry and Sitecore CMS.


How to use:
-----------

Sitecore CMS
-------------------

1. Exclude project Sitecore.SharedModule.ReferenceUpdater.Foundry
2. Save solution and rebuild
3. Copy dlls from respective bin folder and paste in your website bin folder.
4. Copy Sitecore.SharedModule.ReferenceUpdater.CMS.config from Website/App_Config/Include and paste in your website's Website/App_Config/Include folder.


Sitecore Foundry
-----------------------

1. Exclude project Sitecore.SharedModule.ReferenceUpdater.CMS
2. Save solution and rebuild
3. Copy dlls from respective bin folder and paste in your website bin folder.
4. Copy Sitecore.SharedModule.ReferenceUpdater.Foundry.config from Website/App_Config/Include and paste in your website's Website/App_Config/Include folder.

Case covered:
--------------
* Copy
* Clone
* Duplicate
* Add from template or Item from branch.
  
Inspired By:
-------------
http://reasoncodeexample.com/2013/01/13/changing-sitecore-item-references-when-creating-copying-duplicating-and-cloning/ , many thanks to Uli.

Reasons to create:
------------------

We implemented above solution but as number of sites increase we feel slowness in whole environment while copying site.
Thus we need solution which should be very fast,smart and supports multiple sources that makes it flexible for almost all scenario.
  
Fast and Smart:
--------------
It is not using **GetDecendents** API which is very expensive API in case of site copy in Foundry or duplicating whole site.
It only scans destination items(s) to process and only focuses on items which needs update/replaceable means it calculates once if item is ignored then it will be ignored for other items too to save time.
  
Multiple Source:
----------------
In Sitecore foundry while creating site from other site type, we need multiple sources like Site Root, Site Media Root, source path and destination path to update references,
this can be applicable to any multi-site/single site solution.

Supports:
---------
**Sitecore 6.4.0** onwards and **Sitecore Foundry 4.1** as it uses **xmldelta** feature.
  
Thanks to my company [Switch I.T.](http://www.switchit.com) for allowing me to share my work with you all. 

Disclaimer:
-----------

This solution is not tested thoroughly, thus we are not responsible for any damage.

Report Issue:
-------------

Kindly report issue or suggestion if any, I will try my best to help.
