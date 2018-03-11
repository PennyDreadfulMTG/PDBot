
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("AsyncUsage", "AsyncFixer02:Long running or blocking operations under an async method", Justification = "Does not work the way AsyncFixer wants me to do it.", Scope = "member", Target = "~M:PDBot.Core.DiscordFunctions.DoPDHRole~System.Threading.Tasks.Task")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CC0004:Catch block cannot be empty", Justification = "<Pending>", Scope = "member", Target = "~M:PDBot.Discord.DiscordService.Client_MessageReceivedAsync(Discord.WebSocket.SocketMessage)~System.Threading.Tasks.Task")]

