**A simple web scraping program about LPR (Air Ambulance Service), which notifies permitted users via e-mail (SMTP) once the status of the aviation machine changes**

**Current state:**

* checks for the current status of the helicopter and notifies the user once the mission type changes
* changes console colours of the status correspondingly to the colours on the official website
* information with a given date about the outgoing email
* provide XY coordinates with a Google Maps link to a given destination

**To do:**

* use a database to store information about the date of sent e-mails
* use a database to compare statuses instead of fixed statement as it is right now
* provide an executable which is a background process
* allow for simple hosting on web/virtual machine

