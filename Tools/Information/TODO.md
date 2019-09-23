
### Planned features:

 - Command parser.
   - Parse either the start of the input or search for one or more keywords in the input.
   - Bot should have a config for a versioned state of commands. One version could have command X version 1.0.0.0, another could have a higher version and a new command. Bot should be able to switch versions at runtime. This will especially be helpful if I ever implement a plugin system (maybe after .NET Core 3). For this to be useful, any major changes to a command should be forked into it's own new file and registered as a seperate command.
   - Possibly add scripting support to create new commands during runtime. Only doing this if/after I do plugins. Languages will be plugins, so we're not tied to only one.
   - Commands are split up between actors. One actor handles all global-affecting stuff like system and config commands. Other stuff like a markov responder has an actor per guild.
   - Works on signals/messages. No code is run when a new message is received by the bot, instead messages are put into a queue which are then passed as actor messages. The queue is sent and cleared on an interval (every few milliseconds, maybe 100ms), and if it gets too big then a message is sent to report to all actors that if they need to they should spawn more actors to distribute the workload. Actors then get the message to see if they're interested or not, like the database actor saving messages.
   - If the same command is sent multiple times with a frame of time, about 1500ms, the bot will check who sent them to see if any of them is a higher-level user within the bot's command system (admin vs normal), and only execute the higher-level user's command and ignore all others. If all users are the same, the first command is executed and the rest are ignored. This is both to prevent multiple users accidentally attempting to run the same command due to lack of communication, as well as to prevent denial of service attacks if a normal user is attempting to prevent an admin from commanding the bot.
   - "say" command needs an argument to accept a file to attach. only files in a special sub-folder should be usable unless the user is an admin, in which case they can use absolute paths.
   - Each command has it's own database file hiarchy, with a global database that can be overridden for guild-specific databases. This is invisible to the command.
 
 - Markov chain.
   - Support depths 1, 2 and 3.
   - Must cache it's chain in a file and stream the file over the disk. Chain must be able to expand infinitely.
   - Lower and upper limits to how big the chain can be.
   - Do not crash on an empty chain (so many markov libraries do this!)
   - Generate chain in parallel, do not stall any other part of the application. Do not cause resource spikes. Take your time with generating it.
   - Do not attempt to use the new chain until it's done. Do not respond with "Waiting for chain" or queue messages or anything like that. Just ignore.
   - If an older chain exists, use that while waiting for the new one and then switch to it.
   - Rebuild on a regular interval, default 24 hours.
   - If the chain is capped, use the newest messages from the database and discard the oldest.
   - One responder can use multiple chains and be configured on which ones to use at any given event.
 
 - Discord guild history saver.
   - One command to parse and save the entire guild history.
   - No support to skip over any messages. Save the messages, then configure what systems shouldn't use what messages.
   - Does support how long you want to go back or if you only want to read from specific channels.
   - Skips over saving attachments by default so the hard drive doesn't get stuffed saving 3 years worth of messages.
   - Bot will automatically stop collecting from a channel when it hits a message that it aleady stored.
   - Waits for an edited message signal, then saves that new message as a history to the old message. Old message is still saved but now has a message inside of it's message history list.
   - Automatically run on startup, joining a guild or seeing a new channel (in case it was hidden instead of being newly created).
 
 - Configuration on what the bot can and cannot do in specific guilds and channels.
   - Must have commands to edit the config database.
   - Config does not take effect until it is manually reloaded through a command.
   - Examples include being able to read or write to a channel, if it can respond to a user or if it has specific requirements to responding to a user.
   - Bot will not respond to anything and automatically leave the guild if the guild does not have a config. Bot first needs to be set up with a "gen-config \<Guild ID\> \<Main Channel ID\> (Optional: \<Config Template ID\>)" command which generates a config for a guild based off a template and will only respond in the main channel. Channels must be manually added to a whitelist, there is no blacklist support. Superusers registered to the bot can add new channels to the config when they're created.

 - Misc:
   - The bot starts up in "safe mode" where it does nothing but process commands. The system will fully start when given the "init" command. This is useful for configuring the bot in case anything substantial changed that might cause issues if the bot was running.
   - "deployfrom" command which spawns a new process, gets a new binary of the bot from a specified source, shuts down the bot, places the old bot in an "old-(current date)" folder, copies (not moves) the config files, puts the new binaries in the main folder, then optionally starts the new bot, either as-is or in safe mode.
   - Set up architecture so that it can easily be turned into a bunch of tiny plugins if/when plugins are added. This will enable code hot reloading.
   - All guilds will have their own folder and database, all channels will have their own sub-folders and database files.
   - All users will have their own folders with their own config databases. A guild can have a folder and config database for this user that overrides the global one, and a channel-specific config can override them both.
   - Save all attachments from messages. Store them in their own guild-channel-attachments specific database file and have a locally-executed-only command to extract them.
   - Fancy console frontend to view the log and execute commands. Input field will be seperate from the log so you can see what you're typing.
   - Some nice way to generate information about the bot. Maybe generate a fancy HTML page? I don't want to host an entire web server, just generate a local page. No JS or anything. Just report on the current config, version, uptime and total run time, commands registered, resource usage, state of the program and actors, exceptions and errors encountered, database information. Just about everything possible.
   - User database is increment-only. Every time they change their avatar, username or nickname, leave/join a guild the bot is in, or perform an administrative action in the guild, it is recorded and appended to their personal database. Avatars are automatically downloaded and put into their database. All users are checked on startup or joining a guild, and if their current info is different from what's in the database, it's saved, also saved with it is an indication that this was done manually (and the reason for it) and not triggered by a user update event from Discord.
   - Some sort of "personality layer" that commands/configs are based on, so the bot framework can be reused for different projects easily. Bot should be able to run multiple Discord accounts at once.
   - A "port" system to generate and stream audio/synthesized voice over the network. Most (good) voice synthesis solutions are either a seperate server (Festival) or an online service. 
   - I know this is standard with Akka, but we have to make sure that the system can easily communicate with itself if some parts are split up over the network, like if each command or guild actor gets split into it's own process/docker. The goal isn't to actually do that, but it will be nice to have.
   - As usual, the bot will automatically generate all required files and folders that it needs to operate. No seperate installation process or manually creating anything. Closest thing to an installer is the safe mode.
   - For large workloads that can potentially be run in parallel, have the system automatically run benchmarks at runtime to find the optimal threshold for how much work should be done procedurally before it would be better to do it in parallel, then configure the system to use that threshold. This benchmark is cached on-file and won't run again unless the system changes, or a user initiates it manually. This should have the option of being disabled and have the user manually tweak it if they want.
   - Would really like some kind of MUD implemented through a couple commands, for fun. Items, currency, shops, PvP and PvE, etc.
