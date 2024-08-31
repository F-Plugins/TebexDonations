<h1 align="center">TebexDonations</h1>
<h3 align="center">An unofficial plugin for tebex donations</h3>

# Informations

This is an unofficial plugin for tebex donations on your unturned server.
To make it works you have to set up the secret in the configuration, then the plugin will do all for you

## Installation
**Run openmod install Feli.TebexDonations**

### Configuration
```yml
Secret: "" # Here goes your tebex secret key
Endpoint: "https://plugin.buycraft.net/" # Don't change
QueueCheckInterval: 300 # Seconds

StoreURL: "fsurvival.tebex.io" # Your actual store url like fsurvival.tebex.io
StoreCommand: "/donate" # The command a player will execute to go to the store
```

### Commands
- tebex:forcecheck: A command to force check the queue of packages
  id: TebexDonations.Commands.ForceCheckCommand

### Permissions
- Feli.TebexDonations:commands.tebex: Grants access to the TebexDonations.Commands.ForceCheckCommand command.
