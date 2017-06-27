# Slack.NET

## Overview

After having a look at other .NET implementations of the Slack API, it turned out mostly not satisfying.

One is made of blocking methods to run in a separate thread, the other is made of callback-based methods, to develop the JavaScript way... not my `Cup<T>`.

## The goal

This library is being developped in the open, not in a backstage repository with regular public updates. Thus, the repository is going to evolve and the library is going to change, meaning receiving breaking changes until it reaches a stable state.

The goal is to develop a modern Slack library for .NET, hence targeting .NET Standard and using async/await.
It is also planed to make this library GC friendly and performant. Since I'm not a GC and .NET tricks master, if you spot a hidden issue, I'd be happy to learn a lesson.

## Useful links

- .NET Core: https://www.microsoft.com/net/download/core
- Visual Studio: https://www.visualstudio.com/downloads/
- Slack: https://slack.com
- Slack APIs documentation: https://api.slack.com

## For developers

You must have `.NET Core SDK 1.0.4` or higher installed on you development machine. If you use Visual Studio, you will need the version 2017 or higher, edition Community or higher.

The library targets `.NET Standard 1.3`.

If you simply integrate this library into your application, then you have to provide your bot API token to the library. In this case, you are responsible for storing it securely and retrieving it in order to provide it to the library.

However, if you want to try the sample application in the first place, then the first time you run it, it will ask for your bot API token. You can find it in the bot settings of the custom integrations settings of your Slack team, and it is called API token. The sample application stores it encrypted on Windows (using the [`System.Security.Cryptography.ProtectedData`](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata) class) and in clear with `chmod 600` on non-Windows systems.

The DPAPI on Windows is exactly what I want, but is not supported on non-Windows platforms. ASP.NET Core provides some data protection APIs, but are far from being satisfying. You can read more about it here: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection

## For contributors

Contributions are welcome, but I do not have a great experience of multi-contributors project on GitHub.

If you want to fix something, first file an issue describing the problem, so it can become a working base for a pull request and make sure people do not work on the same thing and possibly waste their time.

The generation of XML documentation coupled with warnings treated as error is done on purpose to enforce code to be documented.
Undocumented code, poorly documented code or enforcement deactivation will not be allowed.

Part of the code documentation is taken from the Slack documentation directly.

## How to use

The best way to start is to have a look at the `SlackDotNet.TestApp` project, and particularly the `SlackClient` class.

To integrate the library in order to create your own bot, you fist have to instanciate a `WebApiClient` class, providing it a mandatory API token.
The `WebApiClient` represents the Slack methods web API.

You can immediately call the following method in order to ensure everything is alright and you are using a valid API token.

```
var webApiClient = new WebApiClient("valid-access-token");
Response<AuthTestResponse> authTest = await webApiClient.Auth.Test();
```

Once you have an instance of `WebApiClient`, you can send commands to Slack, but at this point you are only halfway to your goal.
We now need to be informed of events that happen.

For that, create a `RtmApiClient` instance and subscribe to the events you want to handle, as follow:

```
var rtmApiClient = new RtmApiClient();
rtmApiClient.Message += RtmApiClient_Message;

...

private async void RtmApiClient_Message(object sender, MessageInfo message)
{
    ...
}
```

You are now almost done. You have to start the events listening process.
To do so, tell Slack you want to connect to the real time messaging service using the `WebApiClient` calling the `Rtm.Connect` method.
If everything is alright, you get a web socket URL to provide to the `RtmApiClient` calling the `Connect` method.

Hereafter is the sample code for establishing a connection with the real time messaging service, assuming you already have an instance of `WebApiClient` called `webApiClient` and an instance of `RtmApiClient` called `rtmApiClient`.

```
public async Task<bool> Start(int timeout = 5000)
{
    // tells the web api service you want to connect to real time messaging service
    Response<ConnectResponse> connectResponse = await webApiClient.Rtm.Connect();

    // check whether HTTP request succeeded
    if (connectResponse.Status != ExtendedResponseStatus.HttpCallSuccess)
        return false;

    // check wether the service answered positively
    if (connectResponse.ResponseObject.HasError)
        return false;

    // connect to the real time messaging service through web socket
    RtmApiResult x = await rtmApiClient.Connect(connectResponse.ResponseObject.Url, timeout);

    // check whether everything is alright
    if (x.ResultType == RtmApiResultType.Success)
    {
        teamInfo = connectResponse.ResponseObject.Team;
        selfInfo = connectResponse.ResponseObject.Self;
    }

    return true;
}
```

If everything was OK, you should start to receive events. To stop the events listening process, call:

```
rtmApiClient.Message -= RtmApiClient_Message;
rtmApiClient.Disconnect();
```

Do not forget to unsubscribe events.

## Unit tests

There is a unit tests project, but it doesn't really make sense for the moment, since most of the public API surface requires to connect to Slack.
Maybe unit tests will be added later to test internal and private code.

They are made with xUnit, and you can run them from from Visual Studio using the Test Explorer, or from the command line running the following commands:

```
cd SlackDotNet.UnitTests
dotnet test
```

A call to `dotnet restore` might be required before.

## Supported APIs

This library intends to support only Web API and the Real Time Messaging API.

The Events API (https://api.slack.com/events-api) is great on the paper, but requires network / admin setup to redirect ports, which is not always possible, and thus considered to not be supported (at least for now). This is sad for interactive messages, which is a really cool feature, but rely on the Events API, for no obvious reasons.

In the tables bellow, the hyphen `-` means *no*. This is to find the implemented ones more easily.

For now it supports almost nothing, but the library base is set in order to add support to all API step by step in the future.

### Web API

API | Implemented ?
---|---
api.test | yes
auth.revoke | yes
auth.test | yes
bots.info | -
channels.archive | -
channels.create | -
channels.history | -
channels.info | yes
channels.invite | -
channels.join | -
channels.kick | -
channels.leave | -
channels.list | yes
channels.mark | -
channels.rename | -
channels.replies | -
channels.setPurpose | -
channels.setTopic | -
channels.unarchive | -
chat.delete | -
chat.meMessage | -
chat.postMessage | yes
chat.unfurl | -
chat.update | yes
dnd.endDnd | -
dnd.endSnooze | -
dnd.info | -
dnd.setSnooze | -
dnd.teamInfo | -
emoji.list | -
files.comments.add | -
files.comments.delete | -
files.commants.edit | -
files.delete | -
files.info | -
files.list | -
files.revokePublicURL | -
files.sharedPublicURL | -
files.upload | -
groups.archive | -
groups.close | -
groups.create | -
groups.createChild | -
groups.history | -
groups.info | -
groups.invite | -
groups.kick | -
groups.leave | -
groups.list | -
groups.mark | -
groups.open | -
groups.rename | -
groups.replies | -
groups.setPurpose | -
groups.setTopic | -
groups.unarchive | -
im.close | -
im.history | -
im.list | yes
im.mark | -
im.open | -
im.replies | -
mpim.close | -
mpim.history | -
mpim.list | -
mpim.mark | -
mpim.open | -
mpim.replies | -
oauth.access | -
pins.add | -
pins.list | -
pins.remove | -
reactions.add | yes
reactions.get | -
reactions.list | -
reactions.remove | -
reminders.add | -
reminders.complete | -
reminders.delete | -
reminders.info | -
reminders.list | -
rtm.connect | yes
rtm.start | -
search.all | -
search.files | -
search.messages | -
stars.add | -
stars.list | -
stars.remove | -
team.accessLogs | -
team.billableInfo | -
team.info | -
team.integrationLogs | -
team.profile.get | -
usergroups.create | -
usergroups.disable | -
usergroups.enable | -
usergroups.list | -
usergroups.update | -
usergroups.users.list | -
usergroups.users.update | -
users.deletePhoto | -
users.getPresence | -
users.identity | -
users.info | -
users.list | yes
users.setActive | -
users.setPhoto | -
users.setPresence | -
users.profile.get | -
users.profile.set | -

## Real Time Messaging API

API | Implemented ?
---|---
accounts_changed | -
bot_added | -
bot_changed | -
channel_archive | -
channel_created | -
channel_deleted | -
channel_history_changed | -
channel_joined | -
channel_left | -
channel_marked | -
channel_rename | -
channel_unarchive | -
commands_changed | -
dnd_updated | -
dnd_updated_user | -
email_domain_changed | -
emoji_changed | -
file_change | -
file_comment_added | -
file_comment_deleted | -
file_comment_edited | -
file_created | -
file_deleted | -
file_public | -
file_shared | -
file_unshared | -
goodbye | -
group_archive | -
group_close | -
group_history_changed | -
group_joined | -
group_left | -
group_marked | -
group_open | -
group_rename | -
group_unarchive | -
hello | yes
im_close | -
im_created | -
im_history_changed | -
im_marked | -
im_open | -
manual_presence_changed | -
member_joined_channel | -
member_left_channel | -
message | yes
pin_added | -
pin_removed | -
pref_change | -
presence_change | -
presence_sub | -
reaction_added | yes
reaction_removed | -
reconnect_url | -
star_added | -
star_removed | -
subteam_created | -
subteam_members_changed | -
subteam_self_added | -
subteam_self_removed | -
subteam_updated | -
team_domain_change | -
team_join | -
team_migration_started | -
team_plan_change | -
team_pref_change | -
team_profile_change | -
team_profile_delete | -
team_profile_reorder | -
team_rename | -
user_change | -
user_typing | -
