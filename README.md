# wol
a windows wake-on-lan utility to wake a locally attached device using wake-on-lan magic packet protocol.

## Installation

### Winget (recommanded)

type the following command in a windows terminal
> winget install DarkfullDante.wol
### Installer
1. Download the most recent release (wol_setup.exe)
2. Run the installer

#### Manual
1. Clone the repo
2. Place the file /wol/bin/publish/wol.exe in your folder of choice
3. <Optional> Add the location of wol.exe to your environnement variable (PATH) for access from anywhere in cmd/powershell
4. Call the application from its folder ([location]\wol.exe) or from PATH if step 3 was done (wol.exe)

## Usage

The structure of the commands for the tool is has followed:

> wol [OPTIONS...]

### Options
| Option                 | Required | Description                                                                                                                                              |
|------------------------|:--------:|----------------------------------------------------------------------------------------------------------------------------------------------------------|
| -a --address=VALUE     |          | The address to reach the device. Defaults to broadcast (255.255.255.255). Can be IPv4, IPv6 or hostname                                                  |                                                                                                                                      |
| -m --mac=VALUE         | *        | The mac address used to build the magic packet                                                                                                           |                                                                                                                               |
| -p --port=VALUE        |          | The port to use to send the magic packet. Defaults to 7                                                                                                  |
| -d --debug-level=VALUE |          | Verbosity level of the application (0 = silent, 1 = default, 2 = debug)                                                                                  |
| -s --silent            |          | Force the app in silent mode (no text in terminal), equivalent to -d=0                                                                                   |
| -v --version           |          | Shows the installed version of the app                                                                                                                   |
| -h --help              |          | Show the help dialog of the app                                                                                                                          |

A mac address must be provided for the utility to work. All other fields are optional 

### Exemples

#### Minimal run

````powershell
# an obviously inexistent mac address
> wol -m="00:00:00:00:00:00" 
````

#### with ip

````powershell
# an obviously inexistent mac address
> wol -a="192.168.0.230" -m="00:00:00:00:00:00" -p=9
````

#### hostname
````powershell
# an obviously inexistent mac address
> wol -a="hostname.local" -m="00:00:00:00:00:00" -p=9
````

<!--## Contributing-->
## Contributing

Go ahead and make a pull request if you want to contribute.

## Licence
[MIT](LICENSE)
