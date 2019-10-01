
```js
import system
import discord

say <- discord.types.command
say.alias <- "dsay"
say.authorization <- system.authorization.types.username

say.argument.channel <- discord.types.channel
say.argument.channel.onBadArgument <- (msg, badval)
    msg.respond "Error: First argument must be a valid channel ID."

say.onRun <- (msg, channel)
    words <- msg.content.words[2..]
    wait 3s (channel.send words) ? // 'wait' returns true if it timed out
        msg.respond "Error: timeout"
        return
    msg.respond "Success."

say.onUnauthorized <- (msg, _)
    msg.respond "You are not authorized to run this command"

discord.commands <- say
```

The command name is the variable name. Alias is optional, authorization defaults to anyone.
Arguments are also optional. An argument by default is expected to be the nth word of the message
as the nth argument defined in the source file, unless specified otherwise. Example:
`say.argument.channel.placement <- anywhere`. `anywhere` is a reserved word.

Every callback except for `onBadArgument` must have the same number of arguments as the number
of arguments, even if it contains optional arguments. `onBadArgument` instead gets the message
that was sent, as well as the "bad argument" or at least what the system tried to parse as the
argument. Commands also have a `argument.any.onBadArgument` which will run if no other argument
callback is specified for a particular bad argument. For example, if a command has three arguments,
only the middle argument has it's onBadArgument callback set along with the `any` onBadArgument,
and the user invokes the command with a bad first argument, the system will invoke `any`'s
onBadArgument callback.
