# StoryEmails

This mods adds the scrapped E-Mail feature back to the game!
The included e-mails come from official document shared by the game's developers.

![Screenshot 2025-02-16 232310](https://github.com/user-attachments/assets/c158f06f-3d9b-405d-bcf2-949545d58e60)


By default, some e-mails regarding beta elements of the game which did not reach release (Cross-file item transfer for example) are excluded from the inbox. 
To enable them, you will need to change the config setting in the mod's configuration. If you use the [FP2 Mod Manager](github.com/Kuborros/FreedomManager), you should have [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/) preinstalled - in which case pressing **F1** will show you options for all your mods! Otherwise, you will find the config file in your game's install folder, in directory: ``BepInEx/config/``.

E-Mail menu replaces the usual exit button. Not that it did much, as you can exit the pause screen by pressing cancel and start buttons :3

### Important note on what's official and what's not!
Only the **contents** of the built-in E-mails are 'official', as shown in the [Google Docs file](https://docs.google.com/document/d/1xFNWvU0jSlkzFVHznJk8eZTHFm8HGsGPnai8o71H2Pc) shared by the devs on the official GalaxyTrail server.

Every other behaviour, UI element, and even formatting of the text itself was made by me, in an aproximation of how it *could have looked like*. This is **not** how it would have looked like were the feature be completed by GalaxyTrail.

### I want to add custom e-mails!

Great! The mod is made to accept any amount of custom e-mails, all kept in a simple (i hope) JSON file format!
Here is an example of a file adding two e-mails:
```json
[
{
    "subject": "E-Mail 1",
    "body": "This is an e-mail! You can use: \n to add a new line! You can also use most SuperTextMesh tags like <s=1.5>text size</s>, <j>jitter</j> or <w>wobble</w>. If your e-mail gets too long for the window, you can always reduce it's font size! This e-mail will show for everyone, except custom character in excludedModRecipients. Story flag of 0 means it will be always shown in the ui.",
    "from": "someone@domain.com",
    "fromName": "Sender"
    "status": 0,
    "Lilac": false,
    "Carol": false,
    "Milla": false,
    "Neera": false,
    "BaseChars": true,
    "ModdedChars": true,
    "modRecipients": ["custom.character.i.want.to.include.uid","another.one"],
	  "excludedModRecipients": ["but.this.one.should.not.get.it"],
    "hasAttachment": false,
    "attachmentFileName": "",
    "attachmentRealActualPath": "",
    "storyFlag": 8,
    "senderIsNPC": false
},
{
    "subject": "E-Mail 2",
    "body": "This e-mail has an attachment!\nAttachment are searched for based on the path the file is in.\nSo, the provided example will look in a subdirectory called 'Emails', for a file 'Example.png'. Please note that only .png are officially supported. Max size is 320x240, anything above WILL be compressed. This e-mail is set to show up for Lilac, Milla, and Spade. Story flag here means the message will only show once that flag is checked (for us, 8 will be reaching Shang Tu palace for the first time). You can find more story flags on GitHub.",
    "from": "someone@domain.com",
    "fromName": "Sender",
    "status": 0,
    "Lilac": false,
    "Carol": false,
    "Milla": false,
    "Neera": false,
    "BaseChars": false,
    "ModdedChars": false,
    "modRecipients": ["com.kuborro.spade"],
	  "excludedModRecipients": [],
    "hasAttachment": true,
    "attachmentFileName": "funny-name.png",
    "attachmentRealActualPath": "Emails/Example.png",
    "storyFlag": 8,
    "senderIsNPC": false
}
]
```
As you can see, it's a simple JSON array containing JSON objects representing e-mails. While it might look scary with no programming experience, it's as easy as copy pasting the examples and replacing the values! :3
Each file has to start with JSON array start ``[``. Then, after a new line, follow the objects surrounded by ``{`` and ``}``. If there is another object after this one, you need to add ``,`` after it. Last object is followed by ``]`` in new line.

While some fields are self-explanatory (feel free to experiment with them!), below is an explanation of each one:
- "subject": The subject of your message. The space in the UI is bit limited (and shared with the Sender) so don't go on essays here :3
- "body": The contents of your message. You can use: ``\n`` to add a new line! You can also use most [SuperTextMesh html tags](https://supertextmesh.com/docs/SuperTextMesh.html) like ``<s=1.5>text size</s>``, ``<j>jitter</j>``, ``<c=color>color</c>`` or ``<w>wobble</w>``. If your e-mail gets too long for the window, you can always reduce it's font size! Not every effect will work in FP2 - for example no sound-related ones will function, as well as some visual ones.
- "from": Decorative e-mail adress of the sender shown in the e-mail window.
- "fromName": Name of the sender shown on the list and in the e-mail window. If the name belongs to an NPC, it will be used when ``senderIsNPC`` is enabled to check if the message should be delivered.
- "status": Type of the message. ``0`` - Story (gets an e-mail icon). ``1`` - Normal (gets a star icon). ``2`` - Spam (gets a spam icon). ``3`` - Timed (gets a hourglass icon).
- "Lilac": Should Lilac get this e-mail.
- "Carol": Should Carol get this e-mail.
- "Milla": Should Milla get this e-mail.
- "Neera": Should Neera get this e-mail.
- "BaseChars": Should all base game characters get this e-mail. Ignores the options above if set to ``true``.
- "ModdedChars": Should all characters added by mods get this e-mail. Ignores the following list if set to ``true``. Use in conjunction with ``excludedModRecipients`` to exclude _just_ specific characters.
- "modRecipients": List of UIDs of custom characters (registered with such in [FP2Lib](github.com/Kuborros/FP2Lib)) which should get this e-mail.
- "excludedModRecipients": Same format as above, but of characters who should **not** get this e-mail. Only makes sense when used alongside ``ModdedChars`` set to ``true``.
- "hasAttachment": Does this message have an image attachment. Setting this to ``true`` will enable following two fields, and add "Attachment" tab for your e-mail. Please note that users can elect to disable attachments completely, so treat them as optional feature!
- "attachmentFileName": **Decorative** filename of your attachment, shown in the e-mail UI. Not the actual file you are loading. 
- "attachmentRealActualPath": Actual path to the image you wish to load as an attachment. The path is relative to your JSON file (so, in the same folder as it). The image **should be** sized to 320x240px or smaller, and be a .png file.
- "storyFlag": On what Story Flag should the e-mail be delivered. [Here](https://gist.github.com/Kuborros/61da3fde23e74ee2fab3101b026ae0f4) is a file listing all of them.
- "senderIsNPC": If set to ``true``, the e-mail will be only shown if you met the sender NPC (so, talked to them once). Does nothing in Classic Mode.

Once you are done editing, save the file as "ExtraEmails.json". 
- If you are adding it to your existing mod, simply put it in the same folder as your ``modinfo.json``!
- If you are making an addon containing just the e-mails, you will need to package it like a mod - same folder structure, and even ``modinfo.json`` (but you can skip on github links and such ^^). Why? So it can be easily installed and uninstalled! :3

#### Some notes:
- Due to the nature of Unity 5.6's JSON parser, please do not include ``{`` or ``}`` in the contents of the message. It **will** break the parser and skip this message.
- While the image resolution is not enforced and images larger than 320x240 will be loaded, they **will** be forcefully downscaled to that resolution *ignoring the aspect ratio*. Glancing over the fact that this process will scrunkle the image, it will also cause slowdown on weaker machines. Therefore, please use 320x240 (or smaller) images and do not depend on this backup measure unless you really have to.
