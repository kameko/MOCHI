
This is to invoke a command:
```
IMPORT discord

COMMAND say FROM MESSAGE msg OF TYPE discord.types.message
    ALIAS dsay
    ARGUMENT channel IS discord.types.snowflake AT WORD 1
    ARGUMENT words AT WORD 2 TO END
    WHEN INVOKED BY ANYONE AUTHORIZED FOR say
        RESPOND TO channel WITH words
        AND WAIT 3s
            IF TIMEOUT
                RESPOND WITH "Error: timeout"
                AND RETURN
        RESPOND WITH "Success"
    WHEN INVOKED BY ANYONE
        RESPOND WITH "You are not authorized"
    RETURN
```
(Note: `RESPOND WITH ...` is an alias for `RESPOND TO msg.channel WITH ...`)

This is to raise an event:
```
IMPORT discord AS d

LET guilds BE 
    143867839282020352
    196693847965696000

EVENT userjoin 
    WHEN     user  OF TYPE d.types.user
    PERFORMS _     OF TYPE d.actions.join
    IN       guild OF TYPE d.types.guild
        RESPOND TO guild.firstChannelCanSpeakIn WITH "Hello, <?AT user.name>!"
        RETURN
```

Essentially every keyword creates it's own block of code and defines when that block
ends by choosing which new keyword ends it. So `LET` specifically chooses `BE` to end
it's block, creating the variable `guilds` in the global scope. Then `BE` collects
every single element (seperated by spaces, not including inside strings) and places
them inside the variable. Then `BE` chooses to simply let any top-level keyword end it's
scope, such as `LET`, `COMMAND`, or `EVENT`.

All variables are arrays, and when a keyword checks on them, like in `IN guild`, it
actually checks if anything in the array matches. If it finds a match, and there is an
accompanying `CALLED` keyword, it puts the value of the matched element in the variable
name following it.

When a script encounters a `COMMAND` or an `EVENT` it places those events into a queue,
accompanied by the condition it has to meet to run.

Every script is preemptively multitasked, a script in an infinite loop will never block
any other script running on the same thread.

More examples:
```
IMPORT discord AS d

EVENT upgrademsg
    WHEN     user    OF TYPE d.types.user
    PERFORMS msg     OF TYPE d.actions.sendMessage
    IN       channel OF TYPE d.types.channel
    AND msg.content EQUALS "is v5 out yet"
        RESPOND TO guild WITH "<?AT user.name> I did it"
        WAIT UNTIL 800ms
        RESPOND TO guild WITH "sex."
        UNREGISTER upgrademsg
        RETURN
```
