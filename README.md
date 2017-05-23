# follettodellaluce3
Small BOT constantly polling a remote HTTP address, checking for a specific substring, and alerting on telegram for on/off transients. Used to monitor electric power at my home from a remote server. Written in .net core.

## How to run
Copy Config-template.cs to Config.cs, remove the lines starting with '#' and fill all the constants in the file with the appropriate values.

Run the project - whatever way you choose, but a ``dotnet run`` should be enough any platform -- tested on Linux and macOS.

## How to deploy as a systemd service on Linux

* Use ``dotnet publish`` to create a dll file.
* Edit ``fdl3.service`` file; pay attention to all entries in ``[Service]`` section, specially user related ones.
* ``sudo cp fdl3.service /etc/systemd/system/fdl3.service``
* ``sudo systemctl enable fdl3.service`` 
* Start with ``sudo systemctl start fdl3.service`` and stop with ``sudo systemctl stop fdl3.service``
* Use ``sudo journalctl -fu fdl3.service`` to view the systemd logs regarding this service





