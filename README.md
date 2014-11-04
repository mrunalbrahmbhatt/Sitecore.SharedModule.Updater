
This class will updates references of given items, with respect to given source(s) and destination(s).
  
Case covered : Copy, Clone, Duplicate, Add from template, Item from branch.
  
Inspired from : http://reasoncodeexample.com/2013/01/13/changing-sitecore-item-references-when-creating-copying-duplicating-and-cloning/
  
Reasons to create :We implemented above solution but as number of site increase we feel slowness in whole environment while copying site.
Thus we need solution which should be very fast,smart and supports multiple sources that makes it flexible for almost all scenario.
  
Fast and Smart : It is not using GetDecendent API which is very expensive API in case of site copy in Foundry or duplicating whole site.
It only scans destination items(s) to process and only focuses on items which needs update/replaceable means it calculates once if item is ignored then it will be ignored for other items too to save time.
  
Multiple Source : In Sitecore foundry while creating site from other site type, we need multiple source like Site Root, Site Media Root to update references,
this can be applicable to any multi site solution.
  
 