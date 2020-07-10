# LolReplayParser2020
Trying to collect knowledge and successfully parse League of Legends replay files again.

Please open an issue if you have any meaningful info or links to other projects which at any point in time touched on this subject.

My current limitation is lack of knowledge of reverse engineering and bitstream manipulation. As far as I understood from a LoL dev blogpost a while ago they encrypted their client (whatever they meant by that) which I guess means it's way harder to directly reverse the code and get more meaningful info.
Last info was that they were using Blowfish for encrypting the payload and gzip for compressing it, but that might have changed.


# Existing projects and links which touched on this subject:

@loldevs / leaguespec 
https://github.com/loldevs/leaguespec/wiki
https://github.com/loldevs/leaguespec/wiki/Meeting-Summaries

@lukegb lukegb/lolparse.py
https://gist.github.com/lukegb/d2997a5fc7970ce6e1e1

A parser of League of Legends replay files. 
https://github.com/ryancole/LeagueReplayReader

LolSpecAnalyzer is a Java debugging tool/framework for analyzing the League of Legends (http://leagueoflegends.com) spectator data format.
https://github.com/Zero3/LolSpecAnalyzer

@robertabcd / lol-ob - ROFL Container Notes
https://github.com/robertabcd/lol-ob/wiki/ROFL-Container-Notes

@EloGank / lol-replay-downloader 
https://github.com/EloGank/lol-replay-downloader

@leeanchu / ROFL-Player 
https://github.com/leeanchu/ROFL-Player

AutoCaster for Hackathon 2015 by Team Float
https://github.com/protopizza/AutoCaster

@Matviy / LeagueReplayHook 
https://github.com/Matviy/LeagueReplayHook

This is an unofficial, uncomplete and (pretty sure) wrong documentation of the RESTful service which powers the League of Legends spectator mode.
https://gist.github.com/themasch/8375971

https://stackoverflow.com/questions/22827221/league-of-legends-read-chunks-keyframes-through-its-restful-api

https://gamedev.stackexchange.com/questions/49856/league-of-legends-spectator-stream-format

https://www.reddit.com/r/ReverseEngineering/comments/19fqm9/decoding_binary_files/

https://csharp.developreference.com/article/14676731/Blowfish+ECB+encryption+in+C%23+implementation

