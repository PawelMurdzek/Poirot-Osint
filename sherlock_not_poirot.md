# Sherlock vs Poirot - site coverage map

[Sherlock](https://github.com/sherlock-project/sherlock) is a long-running OSS username-enumeration tool that ships a curated `data.json` listing 470+ sites and the heuristic each one needs in order to tell whether a username exists. Poirot OSINT, by contrast, today exercises ~50 platforms across `UsernameSearch.cs` plus a handful of dedicated providers (`BlueskyLookup`, `MastodonLookup`, `GitHubSearch`, `TwitchLookup`, `VkLookup`, etc.). The gap matters because Stage-2 fan-out in `RealSearchService` walks every discovered handle through the username walker, so every site we are missing is a missed identity correlation. This document enumerates the full Sherlock catalog, marks which entries Poirot already handles, and tells the next agent how to close the gap.

## Site list from Sherlock data.json

Source: `https://raw.githubusercontent.com/sherlock-project/sherlock/master/sherlock_project/resources/data.json` (downloaded 2026-04-25, 478 site entries; the `$schema` key is omitted).

Detection column maps Sherlock's `errorType` field. `status_code` = treat any non-2xx as "not found". `message` = parse the body and look for the strings in `errorMsg` (shown in Notes). `response_url` = consider it "not found" if the final URL after redirects equals `errorUrl`.

| Site | urlMain | Detection | Notes |
|------|---------|-----------|-------|
| 1337x | https://www.1337x.to/ | message | url=`https://www.1337x.to/user/{}/`; errorMsg=[<title>Error something went wrong.</title> \| <head><title>404 Not Found</title></head>]; regexCheck=`^[A-Za-z0-9]{4,12}$` |
| 2Dimensions | https://2Dimensions.com/ | status_code | url=`https://2Dimensions.com/a/{}` |
| 7Cups | https://www.7cups.com/ | status_code | url=`https://www.7cups.com/@{}` |
| 9GAG | https://www.9gag.com/ | status_code | url=`https://www.9gag.com/u/{}` |
| About.me | https://about.me/ | status_code | url=`https://about.me/{}` |
| Academia.edu | https://www.academia.edu/ | status_code | url=`https://independent.academia.edu/{}`; regexCheck=`^[^.]*$` |
| addons.wago.io | https://addons.wago.io/ | status_code | url=`https://addons.wago.io/user/{}` |
| AdmireMe.Vip | https://admireme.vip/ | message | url=`https://admireme.vip/{}`; errorMsg="Page Not Found"; NSFW |
| Airbit | https://airbit.com/ | status_code | url=`https://airbit.com/{}` |
| Airliners | https://www.airliners.net/ | status_code | url=`https://www.airliners.net/user/{}/profile/photos` |
| akniga | https://akniga.org/profile/blue/ | status_code | url=`https://akniga.org/profile/{}` |
| All Things Worn | https://www.allthingsworn.com | message | url=`https://www.allthingsworn.com/profile/{}`; errorMsg="Sell Used Panties"; NSFW |
| AllMyLinks | https://allmylinks.com/ | message | url=`https://allmylinks.com/{}`; errorMsg="Page not found"; regexCheck=`^[a-z0-9][a-z0-9-]{2,32}$` |
| Anilist | https://anilist.co/ | status_code | url=`https://anilist.co/user/{}/`; regexCheck=`^[A-Za-z0-9]{2,20}$`; method=POST; has request_payload; urlProbe=`https://graphql.anilist.co/` |
| AniWorld | https://aniworld.to/ | message | url=`https://aniworld.to/user/profil/{}`; errorMsg="Dieses Profil ist nicht verfügbar" |
| Aparat | https://www.aparat.com/ | status_code | url=`https://www.aparat.com/{}/`; method=GET; urlProbe=`https://www.aparat.com/api/fa/v1/user/user/information/username/{}` |
| APClips | https://apclips.com/ | message | url=`https://apclips.com/{}`; errorMsg="Amateur Porn Content Creators"; NSFW |
| Apple Developer | https://developer.apple.com | status_code | url=`https://developer.apple.com/forums/profile/{}` |
| Apple Discussions | https://discussions.apple.com | message | url=`https://discussions.apple.com/profile/{}`; errorMsg="Looking for something in Apple Support Communities?" |
| Archive of Our Own | https://archiveofourown.org/ | status_code | url=`https://archiveofourown.org/users/{}`; regexCheck=`^[^.]*?$` |
| Archive.org | https://archive.org | message | url=`https://archive.org/details/@{}`; errorMsg=[could not fetch an account with user item identifier \| The resource could not be found \| Internet Archive services are temporarily offline]; urlProbe=`https://archive.org/details/@{}?noscript=true`; comment: 'The resource could not be found' relates to archive downtime |
| Arduino Forum | https://forum.arduino.cc/ | status_code | url=`https://forum.arduino.cc/u/{}/summary` |
| ArtStation | https://www.artstation.com/ | status_code | url=`https://www.artstation.com/{}` |
| Asciinema | https://asciinema.org | status_code | url=`https://asciinema.org/~{}` |
| Ask Fedora | https://ask.fedoraproject.org/ | status_code | url=`https://ask.fedoraproject.org/u/{}` |
| Atcoder | https://atcoder.jp/ | status_code | url=`https://atcoder.jp/users/{}` |
| Audiojungle | https://audiojungle.net/ | status_code | url=`https://audiojungle.net/user/{}`; regexCheck=`^[a-zA-Z0-9_]+$` |
| authorSTREAM | http://www.authorstream.com/ | status_code | url=`http://www.authorstream.com/{}/` |
| Autofrage | https://www.autofrage.net/ | status_code | url=`https://www.autofrage.net/nutzer/{}` |
| Avizo | https://www.avizo.cz/ | response_url | url=`https://www.avizo.cz/{}/`; errorUrl=`https://www.avizo.cz/` |
| AWS Skills Profile | https://skillsprofile.skillbuilder.aws | message | url=`https://skillsprofile.skillbuilder.aws/user/{}/`; errorMsg="shareProfileAccepted":false" |
| babyblogRU | https://www.babyblog.ru/ | response_url | url=`https://www.babyblog.ru/user/{}`; errorUrl=`https://www.babyblog.ru/` |
| BabyRu | https://www.baby.ru/ | message | url=`https://www.baby.ru/u/{}`; errorMsg=[Страница, которую вы искали, не найдена \| Доступ с вашего IP-адреса временно ограничен] |
| Bandcamp | https://www.bandcamp.com/ | status_code | url=`https://www.bandcamp.com/{}` |
| Bazar.cz | https://www.bazar.cz/ | response_url | url=`https://www.bazar.cz/{}/`; errorUrl=`https://www.bazar.cz/error404.aspx` |
| Behance | https://www.behance.net/ | status_code | url=`https://www.behance.net/{}` |
| Bezuzyteczna | https://bezuzyteczna.pl | status_code | url=`https://bezuzyteczna.pl/uzytkownicy/{}` |
| BiggerPockets | https://www.biggerpockets.com/ | status_code | url=`https://www.biggerpockets.com/users/{}` |
| BioHacking | https://forum.dangerousthings.com/ | status_code | url=`https://forum.dangerousthings.com/u/{}` |
| BitBucket | https://bitbucket.org/ | status_code | url=`https://bitbucket.org/{}/`; regexCheck=`^[a-zA-Z0-9-_]{1,30}$` |
| Bitwarden Forum | https://bitwarden.com/ | status_code | url=`https://community.bitwarden.com/u/{}/summary`; regexCheck=`^(?![.-])[a-zA-Z0-9_.-]{3,20}$` |
| Blipfoto | https://www.blipfoto.com/ | status_code | url=`https://www.blipfoto.com/{}` |
| Blitz Tactics | https://blitztactics.com/ | message | url=`https://blitztactics.com/{}`; errorMsg="That page doesn't exist" |
| Blogger | https://www.blogger.com/ | status_code | url=`https://{}.blogspot.com`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| Bluesky | https://bsky.app/ | status_code | url=`https://bsky.app/profile/{}.bsky.social`; urlProbe=`https://public.api.bsky.app/xrpc/app.bsky.actor.getProfile?actor={}.bsky.social` |
| BoardGameGeek | https://boardgamegeek.com/ | message | url=`https://boardgamegeek.com/user/{}`; errorMsg=""isValid":true"; urlProbe=`https://api.geekdo.com/api/accounts/validate/username?username={}` |
| BongaCams | https://pt.bongacams.com | status_code | url=`https://pt.bongacams.com/profile/{}`; NSFW |
| Bookcrossing | https://www.bookcrossing.com/ | status_code | url=`https://www.bookcrossing.com/mybookshelf/{}/` |
| BOOTH | https://booth.pm/ | response_url | url=`https://{}.booth.pm/`; errorUrl=`https://booth.pm/`; regexCheck=`^[\w@-]+?$` |
| BraveCommunity | https://community.brave.com/ | status_code | url=`https://community.brave.com/u/{}/` |
| BreachSta.rs Forum | https://breachsta.rs/ | message | url=`https://breachsta.rs/profile/{}`; errorMsg="<title>Error - BreachStars</title>" |
| BugCrowd | https://bugcrowd.com/ | status_code | url=`https://bugcrowd.com/{}` |
| BuyMeACoffee | https://www.buymeacoffee.com/ | status_code | url=`https://buymeacoff.ee/{}`; regexCheck=`[a-zA-Z0-9]{3,15}`; urlProbe=`https://www.buymeacoffee.com/{}` |
| BuzzFeed | https://buzzfeed.com/ | status_code | url=`https://buzzfeed.com/{}` |
| Caddy Community | https://caddy.community/ | status_code | url=`https://caddy.community/u/{}/summary` |
| Car Talk Community | https://community.cartalk.com/ | status_code | url=`https://community.cartalk.com/u/{}/summary` |
| Carbonmade | https://carbonmade.com/ | response_url | url=`https://{}.carbonmade.com`; errorUrl=`https://carbonmade.com/fourohfour?domain={}.carbonmade.com`; regexCheck=`^[\w@-]+?$` |
| Career.habr | https://career.habr.com/ | message | url=`https://career.habr.com/{}`; errorMsg="<h1>Ошибка 404</h1>" |
| CashApp | https://cash.app | status_code | url=`https://cash.app/${}` |
| Cfx.re Forum | https://forum.cfx.re | status_code | url=`https://forum.cfx.re/u/{}/summary` |
| CGTrader | https://www.cgtrader.com | status_code | url=`https://www.cgtrader.com/{}`; regexCheck=`^[^.]*?$` |
| Championat | https://www.championat.com/ | status_code | url=`https://www.championat.com/user/{}` |
| Chaos | https://chaos.social/ | status_code | url=`https://chaos.social/@{}` |
| chaos.social | https://chaos.social/ | status_code | url=`https://chaos.social/@{}` |
| Chatujme.cz | https://chatujme.cz/ | message | url=`https://profil.chatujme.cz/{}`; errorMsg="Neexistujicí profil"; regexCheck=`^[a-zA-Z][a-zA-Z1-9_-]*$` |
| ChaturBate | https://chaturbate.com | status_code | url=`https://chaturbate.com/{}`; NSFW |
| Chess | https://www.chess.com/ | message | url=`https://www.chess.com/member/{}`; errorMsg="Username is valid"; regexCheck=`^[a-zA-Z0-9_]{3,25}$`; urlProbe=`https://www.chess.com/callback/user/valid?username={}` |
| Choice Community | https://choice.community/ | status_code | url=`https://choice.community/u/{}/summary` |
| Chollometro | https://www.chollometro.com/ | status_code | url=`https://www.chollometro.com/profile/{}`; method=GET |
| Clapper | https://clapperapp.com/ | status_code | url=`https://clapperapp.com/{}` |
| CloudflareCommunity | https://community.cloudflare.com/ | status_code | url=`https://community.cloudflare.com/u/{}` |
| Clozemaster | https://www.clozemaster.com | message | url=`https://www.clozemaster.com/players/{}`; errorMsg="Oh no! Player not found." |
| Clubhouse | https://www.clubhouse.com | status_code | url=`https://www.clubhouse.com/@{}` |
| CNET | https://www.cnet.com/ | status_code | url=`https://www.cnet.com/profiles/{}/`; regexCheck=`^[a-z].*$` |
| Code Snippet Wiki | https://codesnippets.fandom.com | message | url=`https://codesnippets.fandom.com/wiki/User:{}`; errorMsg="This user has not filled out their profile page yet" |
| Codeberg | https://codeberg.org/ | status_code | url=`https://codeberg.org/{}` |
| Codecademy | https://www.codecademy.com/ | message | url=`https://www.codecademy.com/profiles/{}`; errorMsg="This profile could not be found" |
| Codechef | https://www.codechef.com/ | response_url | url=`https://www.codechef.com/users/{}`; errorUrl=`https://www.codechef.com/` |
| Codeforces | https://codeforces.com/ | status_code | url=`https://codeforces.com/profile/{}`; urlProbe=`https://codeforces.com/api/user.info?handles={}` |
| Codepen | https://codepen.io/ | status_code | url=`https://codepen.io/{}` |
| Coders Rank | https://codersrank.io/ | message | url=`https://profile.codersrank.io/user/{}/`; errorMsg="not a registered member"; regexCheck=`^[a-zA-Z0-9](?:[a-zA-Z0-9]\|-(?=[a-zA-Z0-9])){0,38}$` |
| Coderwall | https://coderwall.com | status_code | url=`https://coderwall.com/{}` |
| CodeSandbox | https://codesandbox.io | message | url=`https://codesandbox.io/u/{}`; errorMsg="Could not find user with username"; regexCheck=`^[a-zA-Z0-9_-]{3,30}$`; urlProbe=`https://codesandbox.io/api/v1/users/{}` |
| Codewars | https://www.codewars.com | status_code | url=`https://www.codewars.com/users/{}` |
| Codolio | https://codolio.com/ | message | url=`https://codolio.com/profile/{}`; errorMsg="<title>Page Not Found \| Codolio</title>"; regexCheck=`^[a-zA-Z0-9_-]{3,30}$` |
| Coinvote | https://coinvote.cc/ | status_code | url=`https://coinvote.cc/profile/{}` |
| ColourLovers | https://www.colourlovers.com/ | status_code | url=`https://www.colourlovers.com/lover/{}` |
| Contently | https://contently.com/ | response_url | url=`https://{}.contently.com/`; errorUrl=`https://contently.com`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| Coroflot | https://coroflot.com/ | status_code | url=`https://www.coroflot.com/{}` |
| couchsurfing | https://www.couchsurfing.com/ | status_code | url=`https://www.couchsurfing.com/people/{}` |
| Cplusplus | https://cplusplus.com | message | url=`https://cplusplus.com/user/{}`; errorMsg="<title>404 Page Not Found</title>" |
| Cracked | https://www.cracked.com/ | response_url | url=`https://www.cracked.com/members/{}/`; errorUrl=`https://www.cracked.com/` |
| Cracked Forum | https://cracked.sh/ | message | url=`https://cracked.sh/{}`; errorMsg="The member you specified is either invalid or doesn't exist" |
| Credly | https://www.credly.com/ | status_code | url=`https://www.credly.com/users/{}` |
| Crevado | https://crevado.com/ | status_code | url=`https://{}.crevado.com`; regexCheck=`^[\w@-]+?$` |
| Crowdin | https://crowdin.com/ | status_code | url=`https://crowdin.com/profile/{}`; regexCheck=`^[a-zA-Z0-9._-]{2,255}$` |
| CryptoHack | https://cryptohack.org/ | response_url | url=`https://cryptohack.org/user/{}/`; errorUrl=`https://cryptohack.org/` |
| Cryptomator Forum | https://community.cryptomator.org/ | status_code | url=`https://community.cryptomator.org/u/{}` |
| CSSBattle | https://cssbattle.dev | status_code | url=`https://cssbattle.dev/player/{}` |
| CTAN | https://ctan.org/ | status_code | url=`https://ctan.org/author/{}` |
| Cults3D | https://cults3d.com/en | message | url=`https://cults3d.com/en/users/{}/creations`; errorMsg="Oh dear, this page is not working!" |
| CurseForge | https://www.curseforge.com. | status_code | url=`https://www.curseforge.com/members/{}/projects` |
| CyberDefenders | https://cyberdefenders.org/ | status_code | url=`https://cyberdefenders.org/p/{}`; regexCheck=`^[^\/:*?"<>\|@]{3,50}$`; method=GET |
| d3RU | https://d3.ru/ | status_code | url=`https://d3.ru/user/{}/posts` |
| dailykos | https://www.dailykos.com | message | url=`https://www.dailykos.com/user/{}`; errorMsg="{"result":true,"message":null}"; urlProbe=`https://www.dailykos.com/signup/check_nickname?nickname={}` |
| DailyMotion | https://www.dailymotion.com/ | status_code | url=`https://www.dailymotion.com/{}` |
| datingRU | http://dating.ru | status_code | url=`http://dating.ru/{}` |
| dcinside | https://www.dcinside.com/ | status_code | url=`https://gallog.dcinside.com/{}` |
| Dealabs | https://www.dealabs.com/ | message | url=`https://www.dealabs.com/profile/{}`; errorMsg="La page que vous essayez"; regexCheck=`[a-z0-9]{4,16}` |
| DEV Community | https://dev.to/ | status_code | url=`https://dev.to/{}`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| DeviantArt | https://www.deviantart.com/ | message | url=`https://www.deviantart.com/{}`; errorMsg="Llama Not Found"; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| devRant | https://devrant.com/ | response_url | url=`https://devrant.com/users/{}`; errorUrl=`https://devrant.com/` |
| DigitalSpy | https://forums.digitalspy.com/ | message | url=`https://forums.digitalspy.com/profile/{}`; errorMsg="The page you were looking for could not be found."; regexCheck=`^\w{3,20}$` |
| Discogs | https://www.discogs.com/ | status_code | url=`https://www.discogs.com/user/{}` |
| Discord | https://discord.com/ | message | url=`https://discord.com`; errorMsg=[{"taken":false} \| The resource is being rate limited]; method=POST; has request_payload; custom headers; urlProbe=`https://discord.com/api/v9/unique-username/username-attempt-unauthed` |
| Discord.bio | https://discord.bio/ | message | url=`https://discords.com/api-v2/bio/details/{}`; errorMsg="<title>Server Error (500)</title>" |
| Discuss.Elastic.co | https://discuss.elastic.co/ | status_code | url=`https://discuss.elastic.co/u/{}` |
| Diskusjon.no | https://www.diskusjon.no | message | errorMsg="{"result":"ok"}"; regexCheck=`^[a-zA-Z0-9_.-]{3,40}$`; urlProbe=`https://www.diskusjon.no/?app=core&module=system&controller=ajax&do=usernameExists&input={}` |
| Disqus | https://disqus.com/ | status_code | url=`https://disqus.com/{}` |
| DMOJ | https://dmoj.ca/ | message | url=`https://dmoj.ca/user/{}`; errorMsg="No such user" |
| Docker Hub | https://hub.docker.com/ | status_code | url=`https://hub.docker.com/u/{}/`; urlProbe=`https://hub.docker.com/v2/users/{}/` |
| Dribbble | https://dribbble.com/ | message | url=`https://dribbble.com/{}`; errorMsg="Whoops, that page is gone."; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| drive2 | https://www.drive2.ru/ | status_code | url=`https://www.drive2.ru/users/{}` |
| Duolingo | https://duolingo.com/ | message | url=`https://www.duolingo.com/profile/{}`; errorMsg="{"users":[]}"; urlProbe=`https://www.duolingo.com/2017-06-30/users?username={}` |
| eGPU | https://egpu.io/ | status_code | url=`https://egpu.io/forums/profile/{}/` |
| eintracht | https://eintracht.de | status_code | url=`https://community.eintracht.de/fans/{}`; regexCheck=`^[^.]*?$` |
| Eintracht Frankfurt Forum | https://community.eintracht.de/ | status_code | url=`https://community.eintracht.de/fans/{}`; regexCheck=`^[^.]*?$` |
| Empretienda AR | https://empretienda.com | status_code | url=`https://{}.empretienda.com.ar`; comment: Note that Error Connecting responses may be indicative of unclaimed handles |
| Envato Forum | https://forums.envato.com/ | status_code | url=`https://forums.envato.com/u/{}` |
| Erome | https://www.erome.com/ | status_code | url=`https://www.erome.com/{}`; NSFW |
| exophase | https://www.exophase.com/ | status_code | url=`https://www.exophase.com/user/{}/` |
| Exposure | https://exposure.co/ | status_code | url=`https://{}.exposure.co/`; regexCheck=`^[a-zA-Z0-9-]{1,63}$` |
| EyeEm | https://www.eyeem.com/ | status_code | url=`https://www.eyeem.com/u/{}` |
| F3.cool | https://f3.cool/ | status_code | url=`https://f3.cool/{}/` |
| Fameswap | https://fameswap.com/ | status_code | url=`https://fameswap.com/user/{}` |
| Fandom | https://www.fandom.com/ | status_code | url=`https://www.fandom.com/u/{}` |
| Fanpop | https://www.fanpop.com/ | response_url | url=`https://www.fanpop.com/fans/{}`; errorUrl=`https://www.fanpop.com/` |
| Finanzfrage | https://www.finanzfrage.net/ | status_code | url=`https://www.finanzfrage.net/nutzer/{}` |
| fixya | https://www.fixya.com | status_code | url=`https://www.fixya.com/users/{}` |
| fl | https://www.fl.ru/ | status_code | url=`https://www.fl.ru/users/{}` |
| Flickr | https://www.flickr.com/ | status_code | url=`https://www.flickr.com/people/{}` |
| Flightradar24 | https://www.flightradar24.com/ | status_code | url=`https://my.flightradar24.com/{}`; regexCheck=`^[a-zA-Z0-9_]{3,20}$` |
| Flipboard | https://flipboard.com/ | status_code | url=`https://flipboard.com/@{}`; regexCheck=`^([a-zA-Z0-9_]){1,15}$` |
| Football | https://www.rusfootball.info/ | message | url=`https://www.rusfootball.info/user/{}/`; errorMsg="Пользователь с таким именем не найден" |
| FortniteTracker | https://fortnitetracker.com/challenges | status_code | url=`https://fortnitetracker.com/profile/all/{}` |
| Forum Ophilia | https://www.forumophilia.com/ | message | url=`https://www.forumophilia.com/profile.php?mode=viewprofile&u={}`; errorMsg="that user does not exist"; NSFW |
| forum_guns | https://forum.guns.ru/ | message | url=`https://forum.guns.ru/forummisc/blog/{}`; errorMsg="action=https://forum.guns.ru/forummisc/blog/search" |
| Fosstodon | https://fosstodon.org/ | status_code | url=`https://fosstodon.org/@{}`; regexCheck=`^[a-zA-Z0-9_]{1,30}$` |
| Framapiaf | https://framapiaf.org | status_code | url=`https://framapiaf.org/@{}`; regexCheck=`^[a-zA-Z0-9_]{1,30}$` |
| freecodecamp | https://www.freecodecamp.org/ | status_code | url=`https://www.freecodecamp.org/{}`; urlProbe=`https://api.freecodecamp.org/api/users/get-public-profile?username={}` |
| Freelancer | https://www.freelancer.com/ | message | url=`https://www.freelancer.com/u/{}`; errorMsg=""users":{}"; urlProbe=`https://www.freelancer.com/api/users/0.1/users?usernames%5B%5D={}&compact=true` |
| Freesound | https://freesound.org/ | status_code | url=`https://freesound.org/people/{}/` |
| furaffinity | https://www.furaffinity.net | message | url=`https://www.furaffinity.net/user/{}`; errorMsg="This user cannot be found." |
| GaiaOnline | https://www.gaiaonline.com/ | message | url=`https://www.gaiaonline.com/profiles/{}`; errorMsg="No user ID specified or user does not exist" |
| GameFAQs | https://gamefaqs.gamespot.com | status_code | url=`https://gamefaqs.gamespot.com/community/{}` |
| Gamespot | https://www.gamespot.com/ | status_code | url=`https://www.gamespot.com/profile/{}/` |
| GeeksforGeeks | https://www.geeksforgeeks.org/ | status_code | url=`https://auth.geeksforgeeks.org/user/{}` |
| Genius (Artists) | https://genius.com/ | status_code | url=`https://genius.com/artists/{}`; regexCheck=`^[a-zA-Z0-9]{5,50}$` |
| Genius (Users) | https://genius.com/ | status_code | url=`https://genius.com/{}`; regexCheck=`^[a-zA-Z0-9]*?$` |
| geocaching | https://www.geocaching.com/ | status_code | url=`https://www.geocaching.com/p/default.aspx?u={}` |
| Gesundheitsfrage | https://www.gesundheitsfrage.net/ | status_code | url=`https://www.gesundheitsfrage.net/nutzer/{}` |
| GetMyUni | https://getmyuni.com/ | status_code | url=`https://www.getmyuni.com/user/{}` |
| Giant Bomb | https://www.giantbomb.com/ | status_code | url=`https://www.giantbomb.com/profile/{}/` |
| Giphy | https://giphy.com/ | message | url=`https://giphy.com/{}`; errorMsg="<title> GIFs - Find &amp; Share on GIPHY</title>" |
| GitBook | https://gitbook.com/ | status_code | url=`https://{}.gitbook.io/`; regexCheck=`^[\w@-]+?$` |
| Gitea | https://gitea.com/ | status_code | url=`https://gitea.com/{}` |
| Gitee | https://gitee.com/ | status_code | url=`https://gitee.com/{}` |
| GitHub | https://www.github.com/ | status_code | url=`https://www.github.com/{}`; regexCheck=`^[a-zA-Z0-9](?:[a-zA-Z0-9]\|-(?=[a-zA-Z0-9])){0,38}$` |
| GitLab | https://gitlab.com/ | message | url=`https://gitlab.com/{}`; errorMsg="[]"; urlProbe=`https://gitlab.com/api/v4/users?username={}` |
| GNOME VCS | https://gitlab.gnome.org/ | response_url | url=`https://gitlab.gnome.org/{}`; errorUrl=`https://gitlab.gnome.org/{}`; regexCheck=`^(?!-)[a-zA-Z0-9_.-]{2,255}(?<!\.)$` |
| GoodReads | https://www.goodreads.com/ | status_code | url=`https://www.goodreads.com/{}` |
| Google Play | https://play.google.com | message | url=`https://play.google.com/store/apps/developer?id={}`; errorMsg="the requested URL was not found on this server" |
| Gradle | https://gradle.org/ | status_code | url=`https://plugins.gradle.org/u/{}`; regexCheck=`^(?!-)[a-zA-Z0-9-]{3,}(?<!-)$` |
| Grailed | https://www.grailed.com/ | response_url | url=`https://www.grailed.com/{}`; errorUrl=`https://www.grailed.com/{}` |
| Gravatar | http://en.gravatar.com/ | status_code | url=`http://en.gravatar.com/{}`; regexCheck=`^((?!\.).)*$` |
| Gumroad | https://www.gumroad.com/ | message | url=`https://www.gumroad.com/{}`; errorMsg="Page not found (404) - Gumroad"; regexCheck=`^[^.]*?$` |
| Gutefrage | https://www.gutefrage.net/ | status_code | url=`https://www.gutefrage.net/nutzer/{}` |
| habr | https://habr.com/ | status_code | url=`https://habr.com/ru/users/{}` |
| Hackaday | https://hackaday.io/ | status_code | url=`https://hackaday.io/{}` |
| HackenProof (Hackers) | https://hackenproof.com/ | message | url=`https://hackenproof.com/hackers/{}`; errorMsg="Page not found"; regexCheck=`^[\w-]{,34}$` |
| HackerEarth | https://hackerearth.com/ | status_code | url=`https://hackerearth.com/@{}` |
| HackerNews | https://news.ycombinator.com/ | message | url=`https://news.ycombinator.com/user?id={}`; errorMsg=[No such user. \| Sorry.]; comment: First errMsg invalid, second errMsg rate limited. Not ideal. Adjust for bette... |
| HackerOne | https://hackerone.com/ | message | url=`https://hackerone.com/{}`; errorMsg="Page not found" |
| HackerRank | https://hackerrank.com/ | message | url=`https://hackerrank.com/{}`; errorMsg="Something went wrong"; regexCheck=`^[^.]*?$` |
| HackerSploit | https://forum.hackersploit.org/ | status_code | url=`https://forum.hackersploit.org/u/{}` |
| HackMD | https://hackmd.io/ | status_code | url=`https://hackmd.io/@{}` |
| hackster | https://www.hackster.io | status_code | url=`https://www.hackster.io/{}` |
| HackTheBox | https://forum.hackthebox.com/ | status_code | url=`https://forum.hackthebox.com/u/{}` |
| Harvard Scholar | https://scholar.harvard.edu/ | status_code | url=`https://scholar.harvard.edu/{}` |
| Hashnode | https://hashnode.com | status_code | url=`https://hashnode.com/@{}` |
| Heavy-R | https://www.heavy-r.com/ | message | url=`https://www.heavy-r.com/user/{}`; errorMsg="Channel not found"; NSFW |
| Hive Blog | https://hive.blog/ | message | url=`https://hive.blog/@{}`; errorMsg="<title>User Not Found - Hive</title>" |
| Holopin | https://holopin.io | message | url=`https://holopin.io/@{}`; errorMsg="true"; method=POST; has request_payload; urlProbe=`https://www.holopin.io/api/auth/username` |
| HotUKdeals | https://www.hotukdeals.com/ | status_code | url=`https://www.hotukdeals.com/profile/{}`; method=GET |
| Houzz | https://houzz.com/ | status_code | url=`https://houzz.com/user/{}` |
| HubPages | https://hubpages.com/ | status_code | url=`https://hubpages.com/@{}` |
| Hubski | https://hubski.com/ | message | url=`https://hubski.com/user/{}`; errorMsg="No such user" |
| HudsonRock | https://hudsonrock.com | message | url=`https://cavalier.hudsonrock.com/api/json/v2/osint-tools/search-by-username?username={}`; errorMsg="This username is not associated" |
| Hugging Face | https://huggingface.co/ | status_code | url=`https://huggingface.co/{}` |
| hunting | https://www.hunting.ru/forum/ | message | url=`https://www.hunting.ru/forum/members/?username={}`; errorMsg="Указанный пользователь не найден. Пожалуйста, введите другое имя." |
| Icons8 Community | https://community.icons8.com/ | status_code | url=`https://community.icons8.com/u/{}/summary` |
| IFTTT | https://www.ifttt.com/ | status_code | url=`https://www.ifttt.com/p/{}`; regexCheck=`^[A-Za-z0-9]{3,35}$` |
| Ifunny | https://ifunny.co/ | status_code | url=`https://ifunny.co/user/{}` |
| igromania | http://forum.igromania.ru/ | message | url=`http://forum.igromania.ru/member.php?username={}`; errorMsg="Пользователь не зарегистрирован и не имеет профиля для просмотра." |
| Image Fap | https://www.imagefap.com/ | message | url=`https://www.imagefap.com/profile/{}`; errorMsg="Not found"; NSFW |
| ImgUp.cz | https://imgup.cz/ | status_code | url=`https://imgup.cz/{}` |
| Imgur | https://imgur.com/ | status_code | url=`https://imgur.com/user/{}`; urlProbe=`https://api.imgur.com/account/v1/accounts/{}?client_id=546c25a59c58ad7` |
| imood | https://www.imood.com/ | status_code | url=`https://www.imood.com/users/{}` |
| Instagram | https://instagram.com/ | status_code | url=`https://instagram.com/{}`; urlProbe=`https://imginn.com/{}` |
| Instapaper | https://www.instapaper.com/ | status_code | url=`https://www.instapaper.com/p/{}`; method=GET |
| Instructables | https://www.instructables.com/ | status_code | url=`https://www.instructables.com/member/{}`; urlProbe=`https://www.instructables.com/json-api/showAuthorExists?screenName={}` |
| interpals | https://www.interpals.net/ | message | url=`https://www.interpals.net/{}`; errorMsg="The requested user does not exist or is inactive" |
| Intigriti | https://app.intigriti.com | status_code | url=`https://app.intigriti.com/profile/{}`; regexCheck=`[a-z0-9_]{1,25}`; method=GET; urlProbe=`https://api.intigriti.com/user/public/profile/{}` |
| Ionic Forum | https://forum.ionicframework.com/ | status_code | url=`https://forum.ionicframework.com/u/{}` |
| IRC-Galleria | https://irc-galleria.net/ | response_url | url=`https://irc-galleria.net/user/{}`; errorUrl=`https://irc-galleria.net/users/search?username={}` |
| irecommend | https://irecommend.ru/ | status_code | url=`https://irecommend.ru/users/{}` |
| Issuu | https://issuu.com/ | status_code | url=`https://issuu.com/{}` |
| Itch.io | https://itch.io/ | status_code | url=`https://{}.itch.io/`; regexCheck=`^[\w@-]+?$` |
| Itemfix | https://www.itemfix.com/ | message | url=`https://www.itemfix.com/c/{}`; errorMsg="<title>ItemFix - Channel: </title>" |
| jbzd.com.pl | https://jbzd.com.pl/ | status_code | url=`https://jbzd.com.pl/uzytkownik/{}` |
| Jellyfin Weblate | https://translate.jellyfin.org/ | status_code | url=`https://translate.jellyfin.org/user/{}/`; regexCheck=`^[a-zA-Z0-9@._-]{1,150}$` |
| jeuxvideo | https://www.jeuxvideo.com | status_code | url=`https://www.jeuxvideo.com/profil/{}`; method=GET; urlProbe=`https://www.jeuxvideo.com/profil/{}?mode=infos` |
| Jimdo | https://jimdosite.com/ | status_code | url=`https://{}.jimdosite.com`; regexCheck=`^[\w@-]+?$` |
| Joplin Forum | https://discourse.joplinapp.org/ | status_code | url=`https://discourse.joplinapp.org/u/{}` |
| Jupyter Community Forum | https://discourse.jupyter.org | message | url=`https://discourse.jupyter.org/u/{}/summary`; errorMsg="Oops! That page doesn't exist or is private." |
| Kaggle | https://www.kaggle.com/ | status_code | url=`https://www.kaggle.com/{}` |
| kaskus | https://www.kaskus.co.id | status_code | url=`https://www.kaskus.co.id/@{}`; method=GET; urlProbe=`https://www.kaskus.co.id/api/users?username={}` |
| Keybase | https://keybase.io/ | status_code | url=`https://keybase.io/{}` |
| Kick | https://kick.com/ | status_code | url=`https://kick.com/{}`; urlProbe=`https://kick.com/api/v2/channels/{}`; comment: Cloudflare. Only viable when proxied. |
| Kik | http://kik.me/ | message | url=`https://kik.me/{}`; errorMsg="The page you requested was not found"; urlProbe=`https://ws2.kik.com/user/{}` |
| kofi | https://ko-fi.com | response_url | url=`https://ko-fi.com/{}`; errorUrl=`https://ko-fi.com/art?=redirect` |
| Kongregate | https://www.kongregate.com/ | status_code | url=`https://www.kongregate.com/accounts/{}`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$`; custom headers |
| Kvinneguiden | https://forum.kvinneguiden.no | message | errorMsg="{"result":"ok"}"; regexCheck=`^[a-zA-Z0-9_.-]{3,18}$`; urlProbe=`https://forum.kvinneguiden.no/?app=core&module=system&controller=ajax&do=usernameExists&input={}` |
| kwork | https://www.kwork.ru/ | status_code | url=`https://kwork.ru/user/{}` |
| Laracast | https://laracasts.com/ | status_code | url=`https://laracasts.com/@{}`; regexCheck=`^[a-zA-Z0-9_-]{3,}$` |
| last.fm | https://last.fm/ | status_code | url=`https://last.fm/user/{}` |
| Launchpad | https://launchpad.net/ | status_code | url=`https://launchpad.net/~{}` |
| leasehackr | https://forum.leasehackr.com/ | status_code | url=`https://forum.leasehackr.com/u/{}/summary/` |
| LeetCode | https://leetcode.com/ | status_code | url=`https://leetcode.com/{}` |
| LemmyWorld | https://lemmy.world | message | url=`https://lemmy.world/u/{}`; errorMsg="<h1>Error!</h1>" |
| LessWrong | https://www.lesswrong.com/ | response_url | url=`https://www.lesswrong.com/users/{}`; errorUrl=`https://www.lesswrong.com/` |
| Letterboxd | https://letterboxd.com/ | message | url=`https://letterboxd.com/{}`; errorMsg="Sorry, we can't find the page you've requested." |
| LibraryThing | https://www.librarything.com/ | message | url=`https://www.librarything.com/profile/{}`; errorMsg="<p>Error: This user doesn't exist</p>"; custom headers |
| Lichess | https://lichess.org | status_code | url=`https://lichess.org/@/{}` |
| LinkedIn | https://linkedin.com | status_code | url=`https://linkedin.com/in/{}`; regexCheck=`^[a-zA-Z0-9]{3,100}$`; method=GET; custom headers |
| Linktree | https://linktr.ee/ | message | url=`https://linktr.ee/{}`; errorMsg=""statusCode":404"; regexCheck=`^[\w\.]{2,30}$` |
| LinuxFR.org | https://linuxfr.org/ | status_code | url=`https://linuxfr.org/users/{}` |
| Listed | https://listed.to/ | response_url | url=`https://listed.to/@{}`; errorUrl=`https://listed.to/@{}` |
| LiveJournal | https://www.livejournal.com/ | status_code | url=`https://{}.livejournal.com`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| livelib | https://www.livelib.ru/ | status_code | url=`https://www.livelib.ru/reader/{}` |
| Lobsters | https://lobste.rs/ | status_code | url=`https://lobste.rs/u/{}`; regexCheck=`[A-Za-z0-9][A-Za-z0-9_-]{0,24}` |
| LOR | https://linux.org.ru/ | status_code | url=`https://www.linux.org.ru/people/{}/profile` |
| LottieFiles | https://lottiefiles.com/ | status_code | url=`https://lottiefiles.com/{}` |
| LushStories | https://www.lushstories.com/ | status_code | url=`https://www.lushstories.com/profile/{}`; NSFW |
| Mamot | https://mamot.fr/ | status_code | url=`https://mamot.fr/@{}`; regexCheck=`^[a-zA-Z0-9_]{1,30}$` |
| mastodon.cloud | https://mastodon.cloud/ | status_code | url=`https://mastodon.cloud/@{}` |
| mastodon.social | https://chaos.social/ | status_code | url=`https://mastodon.social/@{}` |
| mastodon.xyz | https://mastodon.xyz/ | status_code | url=`https://mastodon.xyz/@{}` |
| Medium | https://medium.com/ | message | url=`https://medium.com/@{}`; errorMsg="<body"; urlProbe=`https://medium.com/feed/@{}` |
| Memrise | https://www.memrise.com/ | status_code | url=`https://www.memrise.com/user/{}/` |
| mercadolivre | https://www.mercadolivre.com.br | status_code | url=`https://www.mercadolivre.com.br/perfil/{}` |
| minds | https://www.minds.com | message | url=`https://www.minds.com/{}/`; errorMsg=""valid":true"; urlProbe=`https://www.minds.com/api/v3/register/validate?username={}` |
| Minecraft | https://minecraft.net/ | message | url=`https://api.mojang.com/users/profiles/minecraft/{}`; errorMsg="Couldn't find any profile with name"; regexCheck=`^.{1,25}$` |
| MixCloud | https://www.mixcloud.com/ | status_code | url=`https://www.mixcloud.com/{}/`; urlProbe=`https://api.mixcloud.com/{}/` |
| MMORPG Forum | https://forums.mmorpg.com/ | status_code | url=`https://forums.mmorpg.com/profile/{}` |
| moikrug | https://moikrug.ru/ | status_code | url=`https://moikrug.ru/{}` |
| Monkeytype | https://monkeytype.com/ | status_code | url=`https://monkeytype.com/profile/{}`; urlProbe=`https://api.monkeytype.com/users/{}/profile` |
| Motherless | https://motherless.com/ | message | url=`https://motherless.com/m/{}`; errorMsg="no longer a member"; NSFW |
| Motorradfrage | https://www.motorradfrage.net/ | status_code | url=`https://www.motorradfrage.net/nutzer/{}` |
| mstdn.io | https://mstdn.io/ | status_code | url=`https://mstdn.io/@{}` |
| mstdn.social | https://mstdn.social/ | status_code | url=`https://mstdn.social/@{}` |
| MuseScore | https://musescore.com/ | status_code | url=`https://musescore.com/{}`; method=GET |
| MyAnimeList | https://myanimelist.net/ | status_code | url=`https://myanimelist.net/profile/{}` |
| Mydealz | https://www.mydealz.de/ | status_code | url=`https://www.mydealz.de/profile/{}`; method=GET |
| Mydramalist | https://mydramalist.com | message | url=`https://www.mydramalist.com/profile/{}`; errorMsg="The requested page was not found" |
| MyMiniFactory | https://www.myminifactory.com/ | status_code | url=`https://www.myminifactory.com/users/{}` |
| Myspace | https://myspace.com/ | status_code | url=`https://myspace.com/{}` |
| n8n Community | https://community.n8n.io/ | status_code | url=`https://community.n8n.io/u/{}/summary` |
| nairaland.com | https://www.nairaland.com/ | status_code | url=`https://www.nairaland.com/{}` |
| namuwiki | https://namu.wiki/ | status_code | url=`https://namu.wiki/w/%EC%82%AC%EC%9A%A9%EC%9E%90:{}`; comment: This is a Korean site and it's expected to return false negatives in certain ... |
| NationStates Nation | https://nationstates.net | message | url=`https://nationstates.net/nation={}`; errorMsg="Was this your nation? It may have ceased to exist due to inactivity, but can rise again!" |
| NationStates Region | https://nationstates.net | message | url=`https://nationstates.net/region={}`; errorMsg="does not exist." |
| Naver | https://naver.com | status_code | url=`https://blog.naver.com/{}` |
| Needrom | https://www.needrom.com/ | status_code | url=`https://www.needrom.com/author/{}/` |
| Newgrounds | https://newgrounds.com | status_code | url=`https://{}.newgrounds.com`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| Nextcloud Forum | https://nextcloud.com/ | status_code | url=`https://help.nextcloud.com/u/{}/summary`; regexCheck=`^(?![.-])[a-zA-Z0-9_.-]{3,20}$` |
| NICommunityForum | https://www.native-instruments.com/forum/ | message | url=`https://community.native-instruments.com/profile/{}`; errorMsg="The page you were looking for could not be found." |
| Nightbot | https://nightbot.tv/ | status_code | url=`https://nightbot.tv/t/{}/commands`; urlProbe=`https://api.nightbot.tv/1/channels/t/{}` |
| Ninja Kiwi | https://ninjakiwi.com/ | response_url | url=`https://ninjakiwi.com/profile/{}`; errorUrl=`https://ninjakiwi.com/profile/{}` |
| NintendoLife | https://www.nintendolife.com/ | status_code | url=`https://www.nintendolife.com/users/{}` |
| NitroType | https://www.nitrotype.com/ | message | url=`https://www.nitrotype.com/racer/{}`; errorMsg="<title>Nitro Type \| Competitive Typing Game \| Race Your Friends</title>" |
| nnRU | https://www.nn.ru/ | status_code | url=`https://{}.www.nn.ru/`; regexCheck=`^[\w@-]+?$` |
| NotABug.org | https://notabug.org/ | status_code | url=`https://notabug.org/{}`; urlProbe=`https://notabug.org/{}/followers` |
| note | https://note.com/ | status_code | url=`https://note.com/{}` |
| Nothing Community | https://nothing.community/ | status_code | url=`https://nothing.community/u/{}` |
| npm | https://www.npmjs.com/ | status_code | url=`https://www.npmjs.com/~{}` |
| Nyaa.si | https://nyaa.si/ | status_code | url=`https://nyaa.si/user/{}` |
| ObservableHQ | https://observablehq.com/ | message | url=`https://observablehq.com/@{}`; errorMsg="Page not found" |
| Odysee | https://odysee.com/ | message | url=`https://odysee.com/@{}`; errorMsg="<link rel="canonical" content="odysee.com"/>" |
| omg.lol | https://home.omg.lol | message | url=`https://{}.omg.lol`; errorMsg=""available": true"; urlProbe=`https://api.omg.lol/address/{}/availability` |
| Open Collective | https://opencollective.com/ | status_code | url=`https://opencollective.com/{}` |
| OpenGameArt | https://opengameart.org | status_code | url=`https://opengameart.org/users/{}` |
| opennet | https://www.opennet.ru/ | message | url=`https://www.opennet.ru/~{}`; errorMsg="Имя участника не найдено"; regexCheck=`^[^-]*$` |
| Opensource | https://opensource.com/ | status_code | url=`https://opensource.com/users/{}` |
| OpenStreetMap | https://www.openstreetmap.org/ | status_code | url=`https://www.openstreetmap.org/user/{}`; regexCheck=`^[^.]*?$` |
| osu! | https://osu.ppy.sh/ | status_code | url=`https://osu.ppy.sh/users/{}` |
| OurDJTalk | https://ourdjtalk.com/ | message | url=`https://ourdjtalk.com/members?username={}`; errorMsg="The specified member cannot be found" |
| Outgress | https://outgress.com/ | message | url=`https://outgress.com/agents/{}`; errorMsg="Outgress - Error" |
| Packagist | https://packagist.org/ | response_url | url=`https://packagist.org/packages/{}/`; errorUrl=`https://packagist.org/search/?q={}&reason=vendor_not_found` |
| Pastebin | https://pastebin.com/ | message | url=`https://pastebin.com/u/{}`; errorMsg="Not Found (#404)" |
| Patched | https://patched.sh/ | message | url=`https://patched.sh/User/{}`; errorMsg="The member you specified is either invalid or doesn't exist." |
| Patreon | https://www.patreon.com/ | status_code | url=`https://www.patreon.com/{}` |
| PCGamer | https://pcgamer.com | message | url=`https://forums.pcgamer.com/members/?username={}`; errorMsg="The specified member cannot be found. Please enter a member's entire name." |
| PentesterLab | https://pentesterlab.com/ | status_code | url=`https://pentesterlab.com/profile/{}`; regexCheck=`^[\w]{4,30}$` |
| Pepperdeals | https://www.pepperdeals.se/ | status_code | url=`https://www.pepperdeals.se/profile/{}`; method=GET |
| PepperealsUS | https://www.pepperdeals.com/ | status_code | url=`https://www.pepperdeals.com/profile/{}`; method=GET |
| PepperNL | https://nl.pepper.com/ | status_code | url=`https://nl.pepper.com/profile/{}`; method=GET |
| PepperPL | https://www.pepper.pl/ | status_code | url=`https://www.pepper.pl/profile/{}`; method=GET |
| Periscope | https://www.periscope.tv/ | status_code | url=`https://www.periscope.tv/{}/` |
| phpRU | https://php.ru/forum/ | message | url=`https://php.ru/forum/members/?username={}`; errorMsg="Указанный пользователь не найден. Пожалуйста, введите другое имя." |
| pikabu | https://pikabu.ru/ | status_code | url=`https://pikabu.ru/@{}` |
| Pinkbike | https://www.pinkbike.com/ | status_code | url=`https://www.pinkbike.com/u/{}/`; regexCheck=`^[^.]*?$` |
| Pinterest | https://www.pinterest.com/ | status_code | url=`https://www.pinterest.com/{}/`; errorUrl=`https://www.pinterest.com/`; urlProbe=`https://www.pinterest.com/oembed.json?url=https://www.pinterest.com/{}/` |
| pixelfed.social | https://pixelfed.social | status_code | url=`https://pixelfed.social/{}/` |
| Platzi | https://platzi.com/ | status_code | url=`https://platzi.com/p/{}/`; method=GET |
| PlayStore | https://play.google.com/store | status_code | url=`https://play.google.com/store/apps/developer?id={}` |
| Playstrategy | https://playstrategy.org | status_code | url=`https://playstrategy.org/@/{}` |
| Plurk | https://www.plurk.com/ | message | url=`https://www.plurk.com/{}`; errorMsg="User Not Found!" |
| PocketStars | https://pocketstars.com/ | message | url=`https://pocketstars.com/{}`; errorMsg="Join Your Favorite Adult Stars"; NSFW |
| Pokemon Showdown | https://pokemonshowdown.com | status_code | url=`https://pokemonshowdown.com/users/{}` |
| Polarsteps | https://polarsteps.com/ | status_code | url=`https://polarsteps.com/{}`; urlProbe=`https://api.polarsteps.com/users/byusername/{}` |
| Polygon | https://www.polygon.com/ | status_code | url=`https://www.polygon.com/users/{}` |
| Polymart | https://polymart.org/ | response_url | url=`https://polymart.org/user/{}`; errorUrl=`https://polymart.org/user/-1` |
| Pornhub | https://pornhub.com/ | status_code | url=`https://pornhub.com/users/{}`; NSFW |
| pr0gramm | https://pr0gramm.com/ | status_code | url=`https://pr0gramm.com/user/{}`; urlProbe=`https://pr0gramm.com/api/profile/info?name={}` |
| Preisjaeger | https://www.preisjaeger.at/ | status_code | url=`https://www.preisjaeger.at/profile/{}`; method=GET |
| ProductHunt | https://www.producthunt.com/ | status_code | url=`https://www.producthunt.com/@{}` |
| prog.hu | https://prog.hu/ | response_url | url=`https://prog.hu/azonosito/info/{}`; errorUrl=`https://prog.hu/azonosito/info/{}` |
| programming.dev | https://programming.dev | message | url=`https://programming.dev/u/{}`; errorMsg="Error!" |
| Promodescuentos | https://www.promodescuentos.com/ | status_code | url=`https://www.promodescuentos.com/profile/{}`; method=GET |
| PromoDJ | http://promodj.com/ | status_code | url=`http://promodj.com/{}` |
| Pronouns.page | https://pronouns.page/ | status_code | url=`https://pronouns.page/@{}` |
| PSNProfiles.com | https://psnprofiles.com/ | response_url | url=`https://psnprofiles.com/{}`; errorUrl=`https://psnprofiles.com/?psnId={}` |
| Pychess | https://www.pychess.org | message | url=`https://www.pychess.org/@/{}`; errorMsg="404" |
| PyPi | https://pypi.org | status_code | url=`https://pypi.org/user/{}`; urlProbe=`https://pypi.org/_includes/administer-user-include/{}` |
| Python.org Discussions | https://discuss.python.org | message | url=`https://discuss.python.org/u/{}/summary`; errorMsg="Oops! That page doesn't exist or is private." |
| Rajce.net | https://www.rajce.idnes.cz/ | status_code | url=`https://{}.rajce.idnes.cz/`; regexCheck=`^[\w@-]+?$` |
| Rarible | https://rarible.com/ | status_code | url=`https://rarible.com/marketplace/api/v4/urls/{}` |
| Rate Your Music | https://rateyourmusic.com/ | status_code | url=`https://rateyourmusic.com/~{}` |
| Rclone Forum | https://forum.rclone.org/ | status_code | url=`https://forum.rclone.org/u/{}` |
| Realmeye | https://www.realmeye.com/ | message | url=`https://www.realmeye.com/player/{}`; errorMsg="Sorry, but we either:" |
| Redbubble | https://www.redbubble.com/ | status_code | url=`https://www.redbubble.com/people/{}` |
| Reddit | https://www.reddit.com/ | message | url=`https://www.reddit.com/user/{}`; errorMsg="Sorry, nobody on Reddit goes by that name."; custom headers |
| RedTube | https://www.redtube.com/ | status_code | url=`https://www.redtube.com/users/{}`; NSFW |
| Reisefrage | https://www.reisefrage.net/ | status_code | url=`https://www.reisefrage.net/nutzer/{}` |
| Replit.com | https://replit.com/ | status_code | url=`https://replit.com/@{}` |
| ResearchGate | https://www.researchgate.net/ | response_url | url=`https://www.researchgate.net/profile/{}`; errorUrl=`https://www.researchgate.net/directory/profiles`; regexCheck=`\w+_\w+` |
| ReverbNation | https://www.reverbnation.com/ | message | url=`https://www.reverbnation.com/{}`; errorMsg="Sorry, we couldn't find that page" |
| Roblox | https://www.roblox.com/ | status_code | url=`https://www.roblox.com/user.aspx?username={}` |
| RocketTube | https://www.rockettube.com/ | message | url=`https://www.rockettube.com/{}`; errorMsg="OOPS! Houston, we have a problem"; NSFW |
| RoyalCams | https://royalcams.com | status_code | url=`https://royalcams.com/profile/{}` |
| Ruby Forums | https://ruby-forums.com | message | url=`https://ruby-forum.com/u/{}/summary`; errorMsg="Oops! That page doesn't exist or is private." |
| RubyGems | https://rubygems.org/ | status_code | url=`https://rubygems.org/profiles/{}`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]{1,40}` |
| Rumble | https://rumble.com/ | status_code | url=`https://rumble.com/user/{}` |
| RuneScape | https://www.runescape.com/ | message | url=`https://apps.runescape.com/runemetrics/app/overview/player/{}`; errorMsg="{"error":"NO_PROFILE","loggedIn":"false"}"; regexCheck=`^(?! )[\w -]{1,12}(?<! )$`; urlProbe=`https://apps.runescape.com/runemetrics/profile/profile?user={}` |
| satsisRU | https://satsis.info/ | status_code | url=`https://satsis.info/user/{}` |
| Sbazar.cz | https://www.sbazar.cz/ | status_code | url=`https://www.sbazar.cz/{}` |
| Scratch | https://scratch.mit.edu/ | status_code | url=`https://scratch.mit.edu/users/{}` |
| Scribd | https://www.scribd.com/ | message | url=`https://www.scribd.com/{}`; errorMsg="Page not found" |
| SEOForum | https://www.seoforum.com/ | status_code | url=`https://seoforum.com/@{}` |
| sessionize | https://sessionize.com/ | status_code | url=`https://sessionize.com/{}` |
| Shelf | https://www.shelf.im/ | status_code | url=`https://www.shelf.im/{}` |
| ShitpostBot5000 | https://www.shitpostbot.com/ | status_code | url=`https://www.shitpostbot.com/user/{}` |
| Signal | https://community.signalusers.org | message | url=`https://community.signalusers.org/u/{}`; errorMsg="Oops! That page doesn't exist or is private." |
| Sketchfab | https://sketchfab.com/ | status_code | url=`https://sketchfab.com/{}` |
| Slack | https://slack.com | status_code | url=`https://{}.slack.com`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| Slant | https://www.slant.co/ | status_code | url=`https://www.slant.co/users/{}`; regexCheck=`^.{2,32}$` |
| Slashdot | https://slashdot.org | message | url=`https://slashdot.org/~{}`; errorMsg="user you requested does not exist" |
| Slides | https://slides.com/ | status_code | url=`https://slides.com/{}` |
| SlideShare | https://slideshare.net/ | message | url=`https://slideshare.net/{}`; errorMsg="<title>Page no longer exists</title>" |
| SmugMug | https://smugmug.com | status_code | url=`https://{}.smugmug.com`; regexCheck=`^[a-zA-Z]{1,35}$` |
| Smule | https://www.smule.com/ | message | url=`https://www.smule.com/{}`; errorMsg="Smule \| Page Not Found (404)" |
| Snapchat | https://www.snapchat.com | status_code | url=`https://www.snapchat.com/add/{}`; regexCheck=`^[a-z][a-z-_.]{3,15}`; method=GET |
| social.tchncs.de | https://social.tchncs.de/ | status_code | url=`https://social.tchncs.de/@{}` |
| SOOP | https://www.sooplive.co.kr/ | status_code | url=`https://www.sooplive.co.kr/station/{}`; urlProbe=`https://api-channel.sooplive.co.kr/v1.1/channel/{}/station` |
| SoundCloud | https://soundcloud.com/ | status_code | url=`https://soundcloud.com/{}` |
| SourceForge | https://sourceforge.net/ | status_code | url=`https://sourceforge.net/u/{}` |
| SoylentNews | https://soylentnews.org | message | url=`https://soylentnews.org/~{}`; errorMsg="The user you requested does not exist, no matter how much you wish this might be the case." |
| SpeakerDeck | https://speakerdeck.com/ | status_code | url=`https://speakerdeck.com/{}` |
| Speedrun.com | https://speedrun.com/ | status_code | url=`https://speedrun.com/users/{}` |
| Spells8 | https://spells8.com | status_code | url=`https://forum.spells8.com/u/{}` |
| spletnik | https://spletnik.ru/ | status_code | url=`https://spletnik.ru/user/{}` |
| Splice | https://splice.com/ | status_code | url=`https://splice.com/{}` |
| Splits.io | https://splits.io | status_code | url=`https://splits.io/users/{}`; regexCheck=`^[^.]*?$` |
| Sporcle | https://www.sporcle.com/ | status_code | url=`https://www.sporcle.com/user/{}/people` |
| Sportlerfrage | https://www.sportlerfrage.net/ | status_code | url=`https://www.sportlerfrage.net/nutzer/{}` |
| SportsRU | https://www.sports.ru/ | status_code | url=`https://www.sports.ru/profile/{}/` |
| Spotify | https://open.spotify.com/ | status_code | url=`https://open.spotify.com/user/{}` |
| Star Citizen | https://robertsspaceindustries.com/ | message | url=`https://robertsspaceindustries.com/citizens/{}`; errorMsg="404" |
| Status Cafe | https://status.cafe/ | message | url=`https://status.cafe/users/{}`; errorMsg="Page Not Found" |
| Steam Community (Group) | https://steamcommunity.com/ | message | url=`https://steamcommunity.com/groups/{}`; errorMsg="No group could be retrieved for the given URL" |
| Steam Community (User) | https://steamcommunity.com/ | message | url=`https://steamcommunity.com/id/{}/`; errorMsg="The specified profile could not be found" |
| Strava | https://www.strava.com/ | status_code | url=`https://www.strava.com/athletes/{}`; regexCheck=`^[^.]*?$` |
| SublimeForum | https://forum.sublimetext.com/ | status_code | url=`https://forum.sublimetext.com/u/{}` |
| svidbook | https://www.svidbook.ru/ | status_code | url=`https://www.svidbook.ru/user/{}` |
| SWAPD | https://swapd.co/ | status_code | url=`https://swapd.co/u/{}` |
| Telegram | https://t.me/ | message | url=`https://t.me/{}`; errorMsg=[<title>Telegram Messenger</title> \| If you have <strong>Telegram</strong>, you can contact <a...]; regexCheck=`^[a-zA-Z0-9_]{3,32}[^_]$` |
| Tellonym.me | https://tellonym.me/ | status_code | url=`https://tellonym.me/{}` |
| Tenor | https://tenor.com/ | status_code | url=`https://tenor.com/users/{}`; regexCheck=`^[A-Za-z0-9_]{2,32}$` |
| Terraria Forums | https://forums.terraria.org/index.php | message | url=`https://forums.terraria.org/index.php?search/42798315/&c[users]={}&o=relevance`; errorMsg="The following members could not be found" |
| TETR.IO | https://tetr.io | message | url=`https://ch.tetr.io/u/{}`; errorMsg="No such user!"; urlProbe=`https://ch.tetr.io/api/users/{}` |
| ThemeForest | https://themeforest.net/ | status_code | url=`https://themeforest.net/user/{}` |
| TheMovieDB | https://www.themoviedb.org/ | status_code | url=`https://www.themoviedb.org/u/{}` |
| threads | https://www.threads.net/ | message | url=`https://www.threads.net/@{}`; errorMsg="<title>Threads • Log in</title>"; custom headers |
| Tiendanube | https://www.tiendanube.com/ | status_code | url=`https://{}.mitiendanube.com/` |
| TikTok | https://www.tiktok.com | message | url=`https://www.tiktok.com/@{}`; errorMsg=["statusCode":10221 \| Govt. of India decided to block 59 apps] |
| tistory | https://www.tistory.com/ | status_code | url=`https://{}.tistory.com/` |
| TnAFlix | https://www.tnaflix.com/ | status_code | url=`https://www.tnaflix.com/profile/{}`; NSFW |
| Topcoder | https://topcoder.com/ | status_code | url=`https://profiles.topcoder.com/{}/`; regexCheck=`^[a-zA-Z0-9_.]+$`; urlProbe=`https://api.topcoder.com/v5/members/{}` |
| Topmate | https://topmate.io/ | status_code | url=`https://topmate.io/{}` |
| toster | https://www.toster.ru/ | status_code | url=`https://www.toster.ru/user/{}/answers` |
| TradingView | https://www.tradingview.com/ | status_code | url=`https://www.tradingview.com/u/{}/`; method=GET |
| Trakt | https://www.trakt.tv/ | status_code | url=`https://www.trakt.tv/users/{}`; regexCheck=`^[^.]*$` |
| TRAKTRAIN | https://traktrain.com/ | status_code | url=`https://traktrain.com/{}` |
| TrashboxRU | https://trashbox.ru/ | status_code | url=`https://trashbox.ru/users/{}`; regexCheck=`^[A-Za-z0-9_-]{3,16}$` |
| Trawelling | https://traewelling.de/ | status_code | url=`https://traewelling.de/@{}` |
| Trello | https://trello.com/ | message | url=`https://trello.com/{}`; errorMsg="model not found"; urlProbe=`https://trello.com/1/Members/{}` |
| Trovo | https://trovo.live | message | url=`https://trovo.live/s/{}/`; errorMsg="Uh Ohhh..." |
| TryHackMe | https://tryhackme.com/ | message | url=`https://tryhackme.com/p/{}`; errorMsg="{"success":false}"; regexCheck=`^[a-zA-Z0-9.]{1,16}$`; urlProbe=`https://tryhackme.com/api/user/exist/{}` |
| tumblr | https://www.tumblr.com/ | status_code | url=`https://{}.tumblr.com/` |
| Tuna | https://tuna.voicemod.net/ | status_code | url=`https://tuna.voicemod.net/user/{}`; regexCheck=`^[a-z0-9]{4,40}$` |
| Tweakers | https://tweakers.net | status_code | url=`https://tweakers.net/gallery/{}` |
| Twitch | https://www.twitch.tv | message | url=`https://www.twitch.tv/{}`; errorMsg="content='Twitch is the world&#39;s leading video platform and community for gamers.'" |
| Twitter | https://x.com/ | message | url=`https://x.com/{}`; errorMsg=[<div class="error-panel"><span>User  \| <title>429 Too Many Requests</title>]; regexCheck=`^[a-zA-Z0-9_]{1,15}$`; urlProbe=`https://nitter.privacydev.net/{}` |
| Typeracer | https://typeracer.com | message | url=`https://data.typeracer.com/pit/profile?user={}`; errorMsg="Profile Not Found" |
| uid | https://uid.me/ | status_code | url=`http://uid.me/{}` |
| Ultimate-Guitar | https://ultimate-guitar.com/ | status_code | url=`https://ultimate-guitar.com/u/{}` |
| Unsplash | https://unsplash.com/ | status_code | url=`https://unsplash.com/@{}`; regexCheck=`^[a-z0-9_]{1,60}$` |
| Untappd | https://untappd.com/ | status_code | url=`https://untappd.com/user/{}` |
| Valorant Forums | https://valorantforums.com | message | url=`https://valorantforums.com/u/{}`; errorMsg="The page you requested could not be found." |
| Velog | https://velog.io/ | status_code | url=`https://velog.io/@{}/posts` |
| Velomania | https://forum.velomania.ru/ | message | url=`https://forum.velomania.ru/member.php?username={}`; errorMsg="Пользователь не зарегистрирован и не имеет профиля для просмотра." |
| Venmo | https://venmo.com/ | message | url=`https://account.venmo.com/u/{}`; errorMsg=[Venmo \| Page Not Found]; custom headers; urlProbe=`https://test1.venmo.com/u/{}` |
| Vero | https://vero.co/ | message | url=`https://vero.co/{}`; errorMsg="Not Found"; method=GET |
| Vimeo | https://vimeo.com/ | status_code | url=`https://vimeo.com/{}` |
| VirusTotal | https://www.virustotal.com/ | status_code | url=`https://www.virustotal.com/gui/user/{}`; method=GET; urlProbe=`https://www.virustotal.com/ui/users/{}/avatar` |
| Vjudge | https://VJudge.net/ | status_code | url=`https://VJudge.net/user/{}` |
| VK | https://vk.com/ | response_url | url=`https://vk.com/{}`; errorUrl=`https://www.quora.com/profile/{}` |
| VLR | https://www.vlr.gg | status_code | url=`https://www.vlr.gg/user/{}` |
| VSCO | https://vsco.co/ | status_code | url=`https://vsco.co/{}` |
| Wakatime | https://wakatime.com/ | status_code | url=`https://wakatime.com/@{}` |
| Warframe Market | https://warframe.market/ | status_code | url=`https://warframe.market/profile/{}`; method=GET; urlProbe=`https://api.warframe.market/v2/user/{}` |
| Warrior Forum | https://www.warriorforum.com/ | status_code | url=`https://www.warriorforum.com/members/{}.html` |
| Wattpad | https://www.wattpad.com/ | status_code | url=`https://www.wattpad.com/user/{}`; urlProbe=`https://www.wattpad.com/api/v3/users/{}/` |
| Weblate | https://hosted.weblate.org/ | status_code | url=`https://hosted.weblate.org/user/{}/`; regexCheck=`^[a-zA-Z0-9@._-]{1,150}$` |
| WebNode | https://www.webnode.cz/ | status_code | url=`https://{}.webnode.cz/`; regexCheck=`^[\w@-]+?$` |
| Weebly | https://weebly.com/ | status_code | url=`https://{}.weebly.com/`; regexCheck=`^[a-zA-Z0-9-]{1,63}$` |
| WICG Forum | https://discourse.wicg.io/ | status_code | url=`https://discourse.wicg.io/u/{}/summary`; regexCheck=`^(?![.-])[a-zA-Z0-9_.-]{3,20}$` |
| Wikidot | http://www.wikidot.com/ | message | url=`http://www.wikidot.com/user:info/{}`; errorMsg="User does not exist." |
| Wikipedia | https://www.wikipedia.org/ | message | url=`https://en.wikipedia.org/wiki/Special:CentralAuth/{}?uselang=qqx`; errorMsg="centralauth-admin-nonexistent:" |
| Windy | https://windy.com/ | status_code | url=`https://community.windy.com/user/{}` |
| Wix | https://wix.com/ | status_code | url=`https://{}.wix.com`; regexCheck=`^[\w@-]+?$` |
| WolframalphaForum | https://community.wolfram.com/ | status_code | url=`https://community.wolfram.com/web/{}/home` |
| Wordnik | https://www.wordnik.com/ | message | url=`https://www.wordnik.com/users/{}`; errorMsg="Page Not Found"; regexCheck=`^[a-zA-Z0-9_.+-]{1,40}$` |
| WordPress | https://wordpress.com | response_url | url=`https://{}.wordpress.com/`; errorUrl=`wordpress.com/typo/?subdomain=`; regexCheck=`^[a-zA-Z][a-zA-Z0-9_-]*$` |
| WordPressOrg | https://wordpress.org/ | response_url | url=`https://profiles.wordpress.org/{}/`; errorUrl=`https://wordpress.org` |
| Wowhead | https://wowhead.com/ | status_code | url=`https://wowhead.com/user={}` |
| write.as | https://write.as | status_code | url=`https://write.as/{}` |
| Wykop | https://www.wykop.pl | status_code | url=`https://www.wykop.pl/ludzie/{}` |
| Xbox Gamertag | https://xboxgamertag.com/ | status_code | url=`https://xboxgamertag.com/search/{}` |
| xHamster | https://xhamster.com | status_code | url=`https://xhamster.com/users/{}`; NSFW; urlProbe=`https://xhamster.com/users/{}?old_browser=true` |
| Xvideos | https://xvideos.com/ | status_code | url=`https://xvideos.com/profiles/{}`; NSFW |
| YandexMusic | https://music.yandex | message | url=`https://music.yandex/users/{}/playlists`; errorMsg=[Ошибка 404 \| <meta name="description" content="Открывайте новую музыку... \| <input type="submit" class="CheckboxCaptcha-Button"]; comment: The first and third errorMsg relate to geo-restrictions and bot detection/cap... |
| YouNow | https://www.younow.com/ | message | url=`https://www.younow.com/{}/`; errorMsg="No users found"; urlProbe=`https://api.younow.com/php/api/broadcast/info/user={}/` |
| YouPic | https://youpic.com/ | status_code | url=`https://youpic.com/photographer/{}/` |
| YouPorn | https://youporn.com | status_code | url=`https://youporn.com/uservids/{}`; NSFW |
| YouTube | https://www.youtube.com/ | status_code | url=`https://www.youtube.com/@{}` |
| znanylekarz.pl | https://znanylekarz.pl | status_code | url=`https://www.znanylekarz.pl/{}` |

## Coverage cross-reference

Mapping every Sherlock entry to Poirot. `covered` = the same platform is already polled by `UsernameSearch.cs` or a dedicated provider. `partial` = Poirot has a related provider but the URL pattern or detection logic is different. `missing` = no Poirot coverage today.

| Site | Already in Poirot? | Status |
|------|--------------------|--------|
| 1337x | - | missing |
| 2Dimensions | - | missing |
| 7Cups | - | missing |
| 9GAG | - | missing |
| About.me | UsernameSearch | covered |
| Academia.edu | - | missing |
| addons.wago.io | - | missing |
| AdmireMe.Vip | - | missing |
| Airbit | - | missing |
| Airliners | - | missing |
| akniga | - | missing |
| All Things Worn | - | missing |
| AllMyLinks | - | missing |
| Anilist | - | missing |
| AniWorld | - | missing |
| Aparat | - | missing |
| APClips | - | missing |
| Apple Developer | - | missing |
| Apple Discussions | - | missing |
| Archive of Our Own | - | missing |
| Archive.org | - | missing |
| Arduino Forum | - | missing |
| ArtStation | - | missing |
| Asciinema | - | missing |
| Ask Fedora | - | missing |
| Atcoder | - | missing |
| Audiojungle | - | missing |
| authorSTREAM | - | missing |
| Autofrage | - | missing |
| Avizo | - | missing |
| AWS Skills Profile | - | missing |
| babyblogRU | - | missing |
| BabyRu | - | missing |
| Bandcamp | UsernameSearch | covered |
| Bazar.cz | - | missing |
| Behance | UsernameSearch | covered |
| Bezuzyteczna | - | missing |
| BiggerPockets | - | missing |
| BioHacking | - | missing |
| BitBucket | UsernameSearch | covered |
| Bitwarden Forum | - | missing |
| Blipfoto | - | missing |
| Blitz Tactics | - | missing |
| Blogger | - | missing |
| Bluesky | - | missing |
| BoardGameGeek | - | missing |
| BongaCams | - | missing |
| Bookcrossing | - | missing |
| BOOTH | - | missing |
| BraveCommunity | - | missing |
| BreachSta.rs Forum | - | missing |
| BugCrowd | - | missing |
| BuyMeACoffee | UsernameSearch | covered |
| BuzzFeed | - | missing |
| Caddy Community | - | missing |
| Car Talk Community | - | missing |
| Carbonmade | - | missing |
| Career.habr | - | missing |
| CashApp | - | missing |
| Cfx.re Forum | - | missing |
| CGTrader | - | missing |
| Championat | - | missing |
| Chaos | - | missing |
| chaos.social | - | missing |
| Chatujme.cz | - | missing |
| ChaturBate | - | missing |
| Chess | UsernameSearch | covered |
| Choice Community | - | missing |
| Chollometro | - | missing |
| Clapper | - | missing |
| CloudflareCommunity | - | missing |
| Clozemaster | - | missing |
| Clubhouse | - | missing |
| CNET | - | missing |
| Code Snippet Wiki | - | missing |
| Codeberg | - | missing |
| Codecademy | - | missing |
| Codechef | - | missing |
| Codeforces | - | missing |
| Codepen | UsernameSearch | covered |
| Coders Rank | - | missing |
| Coderwall | - | missing |
| CodeSandbox | - | missing |
| Codewars | - | missing |
| Codolio | - | missing |
| Coinvote | - | missing |
| ColourLovers | - | missing |
| Contently | - | missing |
| Coroflot | - | missing |
| couchsurfing | - | missing |
| Cplusplus | - | missing |
| Cracked | - | missing |
| Cracked Forum | - | missing |
| Credly | - | missing |
| Crevado | - | missing |
| Crowdin | - | missing |
| CryptoHack | - | missing |
| Cryptomator Forum | - | missing |
| CSSBattle | - | missing |
| CTAN | - | missing |
| Cults3D | - | missing |
| CurseForge | - | missing |
| CyberDefenders | - | missing |
| d3RU | - | missing |
| dailykos | - | missing |
| DailyMotion | - | missing |
| datingRU | - | missing |
| dcinside | - | missing |
| Dealabs | - | missing |
| DEV Community | UsernameSearch + DevToLookup (urlMain match) | partial |
| DeviantArt | UsernameSearch | covered |
| devRant | - | missing |
| DigitalSpy | - | missing |
| Discogs | - | missing |
| Discord | UsernameSearch | covered |
| Discord.bio | UsernameSearch (urlMain match) | partial |
| Discuss.Elastic.co | - | missing |
| Diskusjon.no | - | missing |
| Disqus | - | missing |
| DMOJ | - | missing |
| Docker Hub | - | missing |
| Dribbble | UsernameSearch | covered |
| drive2 | - | missing |
| Duolingo | - | missing |
| eGPU | - | missing |
| eintracht | - | missing |
| Eintracht Frankfurt Forum | - | missing |
| Empretienda AR | - | missing |
| Envato Forum | - | missing |
| Erome | - | missing |
| exophase | - | missing |
| Exposure | - | missing |
| EyeEm | - | missing |
| F3.cool | - | missing |
| Fameswap | - | missing |
| Fandom | - | missing |
| Fanpop | - | missing |
| Finanzfrage | - | missing |
| fixya | - | missing |
| fl | - | missing |
| Flickr | UsernameSearch | covered |
| Flightradar24 | - | missing |
| Flipboard | - | missing |
| Football | - | missing |
| FortniteTracker | - | missing |
| Forum Ophilia | - | missing |
| forum_guns | - | missing |
| Fosstodon | - | missing |
| Framapiaf | - | missing |
| freecodecamp | - | missing |
| Freelancer | - | missing |
| Freesound | - | missing |
| furaffinity | - | missing |
| GaiaOnline | - | missing |
| GameFAQs | - | missing |
| Gamespot | - | missing |
| GeeksforGeeks | - | missing |
| Genius (Artists) | - | missing |
| Genius (Users) | - | missing |
| geocaching | - | missing |
| Gesundheitsfrage | - | missing |
| GetMyUni | - | missing |
| Giant Bomb | - | missing |
| Giphy | - | missing |
| GitBook | - | missing |
| Gitea | - | missing |
| Gitee | - | missing |
| GitHub | GitHubSearch + UsernameSearch | covered |
| GitLab | GitLabSearch + UsernameSearch | covered |
| GNOME VCS | GitLabSearch + UsernameSearch (urlMain match) | partial |
| GoodReads | - | missing |
| Google Play | - | missing |
| Gradle | - | missing |
| Grailed | - | missing |
| Gravatar | UsernameSearch + GravatarLookup | covered |
| Gumroad | - | missing |
| Gutefrage | - | missing |
| habr | - | missing |
| Hackaday | - | missing |
| HackenProof (Hackers) | - | missing |
| HackerEarth | - | missing |
| HackerNews | HackerNewsLookup | covered |
| HackerOne | - | missing |
| HackerRank | UsernameSearch + HackerRankLookup | covered |
| HackerSploit | - | missing |
| HackMD | - | missing |
| hackster | - | missing |
| HackTheBox | - | missing |
| Harvard Scholar | - | missing |
| Hashnode | UsernameSearch | covered |
| Heavy-R | - | missing |
| Hive Blog | - | missing |
| Holopin | - | missing |
| HotUKdeals | - | missing |
| Houzz | - | missing |
| HubPages | - | missing |
| Hubski | - | missing |
| HudsonRock | - | missing |
| Hugging Face | - | missing |
| hunting | - | missing |
| Icons8 Community | - | missing |
| IFTTT | - | missing |
| Ifunny | - | missing |
| igromania | - | missing |
| Image Fap | - | missing |
| ImgUp.cz | - | missing |
| Imgur | - | missing |
| imood | - | missing |
| Instagram | UsernameSearch | covered |
| Instapaper | - | missing |
| Instructables | - | missing |
| interpals | - | missing |
| Intigriti | - | missing |
| Ionic Forum | - | missing |
| IRC-Galleria | - | missing |
| irecommend | - | missing |
| Issuu | - | missing |
| Itch.io | - | missing |
| Itemfix | - | missing |
| jbzd.com.pl | - | missing |
| Jellyfin Weblate | - | missing |
| jeuxvideo | - | missing |
| Jimdo | - | missing |
| Joplin Forum | - | missing |
| Jupyter Community Forum | - | missing |
| Kaggle | UsernameSearch | covered |
| kaskus | - | missing |
| Keybase | UsernameSearch | covered |
| Kick | - | missing |
| Kik | - | missing |
| kofi | UsernameSearch | covered |
| Kongregate | - | missing |
| Kvinneguiden | - | missing |
| kwork | - | missing |
| Laracast | - | missing |
| last.fm | UsernameSearch | covered |
| Launchpad | - | missing |
| leasehackr | - | missing |
| LeetCode | UsernameSearch | covered |
| LemmyWorld | LemmyLookup (urlMain match) | partial |
| LessWrong | - | missing |
| Letterboxd | - | missing |
| LibraryThing | - | missing |
| Lichess | - | missing |
| LinkedIn | UsernameSearch | covered |
| Linktree | - | missing |
| LinuxFR.org | - | missing |
| Listed | - | missing |
| LiveJournal | - | missing |
| livelib | - | missing |
| Lobsters | - | missing |
| LOR | - | missing |
| LottieFiles | - | missing |
| LushStories | - | missing |
| Mamot | - | missing |
| mastodon.cloud | MastodonLookup (urlMain match) | partial |
| mastodon.social | - | missing |
| mastodon.xyz | MastodonLookup (urlMain match) | partial |
| Medium | UsernameSearch | covered |
| Memrise | - | missing |
| mercadolivre | - | missing |
| minds | - | missing |
| Minecraft | - | missing |
| MixCloud | - | missing |
| MMORPG Forum | - | missing |
| moikrug | - | missing |
| Monkeytype | - | missing |
| Motherless | - | missing |
| Motorradfrage | - | missing |
| mstdn.io | - | missing |
| mstdn.social | - | missing |
| MuseScore | - | missing |
| MyAnimeList | UsernameSearch | covered |
| Mydealz | - | missing |
| Mydramalist | - | missing |
| MyMiniFactory | - | missing |
| Myspace | - | missing |
| n8n Community | - | missing |
| nairaland.com | - | missing |
| namuwiki | - | missing |
| NationStates Nation | - | missing |
| NationStates Region | - | missing |
| Naver | - | missing |
| Needrom | - | missing |
| Newgrounds | - | missing |
| Nextcloud Forum | - | missing |
| NICommunityForum | - | missing |
| Nightbot | - | missing |
| Ninja Kiwi | - | missing |
| NintendoLife | - | missing |
| NitroType | - | missing |
| nnRU | - | missing |
| NotABug.org | - | missing |
| note | - | missing |
| Nothing Community | - | missing |
| npm | UsernameSearch | covered |
| Nyaa.si | - | missing |
| ObservableHQ | - | missing |
| Odysee | - | missing |
| omg.lol | - | missing |
| Open Collective | - | missing |
| OpenGameArt | - | missing |
| opennet | - | missing |
| Opensource | - | missing |
| OpenStreetMap | - | missing |
| osu! | - | missing |
| OurDJTalk | - | missing |
| Outgress | - | missing |
| Packagist | - | missing |
| Pastebin | - | missing |
| Patched | - | missing |
| Patreon | UsernameSearch | covered |
| PCGamer | - | missing |
| PentesterLab | - | missing |
| Pepperdeals | - | missing |
| PepperealsUS | - | missing |
| PepperNL | - | missing |
| PepperPL | - | missing |
| Periscope | - | missing |
| phpRU | - | missing |
| pikabu | - | missing |
| Pinkbike | - | missing |
| Pinterest | UsernameSearch | covered |
| pixelfed.social | - | missing |
| Platzi | - | missing |
| PlayStore | - | missing |
| Playstrategy | - | missing |
| Plurk | - | missing |
| PocketStars | - | missing |
| Pokemon Showdown | - | missing |
| Polarsteps | - | missing |
| Polygon | - | missing |
| Polymart | - | missing |
| Pornhub | - | missing |
| pr0gramm | - | missing |
| Preisjaeger | - | missing |
| ProductHunt | - | missing |
| prog.hu | - | missing |
| programming.dev | - | missing |
| Promodescuentos | - | missing |
| PromoDJ | - | missing |
| Pronouns.page | - | missing |
| PSNProfiles.com | - | missing |
| Pychess | - | missing |
| PyPi | UsernameSearch | covered |
| Python.org Discussions | - | missing |
| Rajce.net | - | missing |
| Rarible | - | missing |
| Rate Your Music | - | missing |
| Rclone Forum | - | missing |
| Realmeye | - | missing |
| Redbubble | - | missing |
| Reddit | UsernameSearch + RedditDiscovery | covered |
| RedTube | - | missing |
| Reisefrage | - | missing |
| Replit.com | UsernameSearch (urlMain match) | partial |
| ResearchGate | - | missing |
| ReverbNation | - | missing |
| Roblox | UsernameSearch | covered |
| RocketTube | - | missing |
| RoyalCams | - | missing |
| Ruby Forums | - | missing |
| RubyGems | - | missing |
| Rumble | - | missing |
| RuneScape | - | missing |
| satsisRU | - | missing |
| Sbazar.cz | - | missing |
| Scratch | - | missing |
| Scribd | - | missing |
| SEOForum | - | missing |
| sessionize | - | missing |
| Shelf | - | missing |
| ShitpostBot5000 | - | missing |
| Signal | - | missing |
| Sketchfab | - | missing |
| Slack | - | missing |
| Slant | - | missing |
| Slashdot | - | missing |
| Slides | - | missing |
| SlideShare | - | missing |
| SmugMug | - | missing |
| Smule | - | missing |
| Snapchat | - | missing |
| social.tchncs.de | - | missing |
| SOOP | - | missing |
| SoundCloud | UsernameSearch | covered |
| SourceForge | - | missing |
| SoylentNews | - | missing |
| SpeakerDeck | - | missing |
| Speedrun.com | - | missing |
| Spells8 | - | missing |
| spletnik | - | missing |
| Splice | - | missing |
| Splits.io | - | missing |
| Sporcle | - | missing |
| Sportlerfrage | - | missing |
| SportsRU | - | missing |
| Spotify | UsernameSearch | covered |
| Star Citizen | - | missing |
| Status Cafe | - | missing |
| Steam Community (Group) | UsernameSearch (urlMain match) | partial |
| Steam Community (User) | UsernameSearch (urlMain match) | partial |
| Strava | - | missing |
| SublimeForum | - | missing |
| svidbook | - | missing |
| SWAPD | - | missing |
| Telegram | UsernameSearch + TelegramLookup | covered |
| Tellonym.me | - | missing |
| Tenor | - | missing |
| Terraria Forums | - | missing |
| TETR.IO | - | missing |
| ThemeForest | - | missing |
| TheMovieDB | - | missing |
| threads | - | missing |
| Tiendanube | - | missing |
| TikTok | UsernameSearch | covered |
| tistory | - | missing |
| TnAFlix | - | missing |
| Topcoder | - | missing |
| Topmate | - | missing |
| toster | - | missing |
| TradingView | - | missing |
| Trakt | - | missing |
| TRAKTRAIN | - | missing |
| TrashboxRU | - | missing |
| Trawelling | - | missing |
| Trello | - | missing |
| Trovo | - | missing |
| TryHackMe | - | missing |
| tumblr | UsernameSearch | covered |
| Tuna | - | missing |
| Tweakers | - | missing |
| Twitch | UsernameSearch + TwitchLookup | covered |
| Twitter | UsernameSearch (X/Twitter) | covered |
| Typeracer | - | missing |
| uid | - | missing |
| Ultimate-Guitar | - | missing |
| Unsplash | - | missing |
| Untappd | - | missing |
| Valorant Forums | - | missing |
| Velog | - | missing |
| Velomania | - | missing |
| Venmo | - | missing |
| Vero | - | missing |
| Vimeo | - | missing |
| VirusTotal | - | missing |
| Vjudge | - | missing |
| VK | UsernameSearch + VkLookup | covered |
| VLR | - | missing |
| VSCO | - | missing |
| Wakatime | - | missing |
| Warframe Market | - | missing |
| Warrior Forum | - | missing |
| Wattpad | - | missing |
| Weblate | - | missing |
| WebNode | - | missing |
| Weebly | - | missing |
| WICG Forum | - | missing |
| Wikidot | - | missing |
| Wikipedia | - | missing |
| Windy | - | missing |
| Wix | - | missing |
| WolframalphaForum | - | missing |
| Wordnik | - | missing |
| WordPress | - | missing |
| WordPressOrg | - | missing |
| Wowhead | - | missing |
| write.as | - | missing |
| Wykop | WykopLookup | covered |
| Xbox Gamertag | UsernameSearch (Xbox) | covered |
| xHamster | - | missing |
| Xvideos | - | missing |
| YandexMusic | - | missing |
| YouNow | - | missing |
| YouPic | - | missing |
| YouPorn | - | missing |
| YouTube | UsernameSearch + YouTubeDiscovery | covered |
| znanylekarz.pl | - | missing |

Totals: 43 covered, 9 partial, 426 missing (out of 478 total).

## Implementation brief for the next agent

You are inheriting roughly four hundred missing sites. Do not panic and do not try to write four hundred providers. Start by extending `src/SherlockOsint.Api/Services/OsintProviders/UsernameSearch.cs`: walk the table above, take every Sherlock entry whose `Detection` column says `status_code` (about half), and add it as a new `PlatformCheck` record in the `Platforms` list. The existing semaphore (20 concurrent) handles the load, and the `ValidateGeneric` fallback already ships with reasonable heuristics. For entries whose detection is `message`, add a tiny per-site validator in `ValidateProfileExists`'s switch and have it grep for the strings from the `errorMsg` field - failure to match means the user exists.

Reach for a dedicated provider only when a site offers a structured JSON API (Bluesky AppView, Mastodon `.well-known/webfinger`, GitHub `users/{login}`, Lemmy `api/v3/user`). New providers live next to the existing ones in `src/SherlockOsint.Api/Services/OsintProviders/`, expose either `Task<IEnumerable<OsintNode>> SearchAsync(string nick, CancellationToken ct)` or an `IAsyncEnumerable<OsintNode>` variant, get registered as a singleton in `Program.cs`, and must be wired into `RealSearchService.RunSearchRoundAsync` (around line 229) so Stage-2 fan-out actually invokes them. Use `IHttpClientFactory.CreateClient("OsintClient")` to inherit the 10-second timeout and shared retry policy. Skip silently when an API key is absent - that is the convention every other provider follows.

One critical gotcha from `todo.md`: an HTTP 200 only proves the username string is registered, never that the same human owns it. Username collisions across platforms are the rule, not the exception. After you add new sites, route every fresh node through `ProfileVerifier.cs` so the LLM scoring step in `CandidateAggregator.BuildCandidatesAsync` weights real evidence (display name, location, bio overlap) and not raw URL hits. Otherwise you will flood `ClaudeAnalysisService` with noise and the candidate probabilities will collapse toward 50%.
