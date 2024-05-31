# Disc_Control
Disc_Control is a console application that finds all drives on the computer and shows their information. The application uses the percentual free space on the disk to determine what kind of event log it should write. It also uses the percentual free space to determine if a threshold has been passed and reacts accordingly.

To show the information about drives in a proper way, the console application creates a web server on localhost. The web server also shows the current configuration and creates a separate card for each drive.

There are three configuration files that are automatically created in the bin directory. One of them contains the main configurations (thresholds, what is shown, etc.), the second one has specific configurations for drives, and the last one is for the web server, allowing you to edit the appearance of the output.
