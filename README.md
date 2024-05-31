# Disc_Control
Disc_Control is a console application that finds all drives on the computer and displays their information. The application uses the percentual free space on the disk to determine what kind of event log it should write. It also uses the percentual free space to determine if a configurable threshold has been passed and reacts accordingly.

To display the information about drives properly, the console application creates a web server on localhost. The web server also shows the current configuration and creates a separate card for each drive.

# Config files
There are three configuration files that are automatically created in the bin directory:

1. config.json: This file contains the main configurations, such as the interval, critical threshold, warning threshold, and web server port. It also allows you to choose whether to use the same critical threshold for every drive or specific thresholds set in drivesconfig.json. Additionally, you can configure whether the console will display a short or long version of the information, show unready drives, and show network drives.

2. drivesconfig.json: This file contains specific configurations for individual drives.

3. main.less: This file allows you to edit the appearance of the web server output.
