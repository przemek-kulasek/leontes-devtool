As a senior .NET developer and architect I am now heavly using AI in my day-to-day working routine.

I have my flow:
1. Reading jira task.
2. Copy jira task description and most important informations into newly created folder named as story key for example MA-777 into spec.md file.
3. In this folder creating images folder images and copying designs, screenshots etc.
4. In spec.md I'm adding necessary materials/projects on my disk, for example in my current project I'm adding location to legacy app db structure scipts, legacy app desktop, new app api/backend, new app frontend.
5. I'm adding info in my spec.md: 1. Do not ask about trivial things you can find in given materials/locations. 2. Always ask if something is unclear and you cannot find in materials. Never make assumptions or hallucinate. 3. If not exactly specified in your context already use clean code, dry, yagni, solid principles, all codebase must be the same style as the rest of the project, use same patterns etc. 4. Always ask about using external packages and present info about licence. 5. Do not add tests at this stage.
6. I'm asking llm about analyzing all materials, prepare plan to accept and ask about unclear things.
7. Then it is implementing the feature.
8. I'm testing if this works and asking about changes etc.
9. Once I have initial working version I'm asking to do the code review (using skill mostly) and use good practices as mentioned above.
10. I'm asking to call github to get closed prs for specific project to see if we are not breaking any from the past comments (written by human not copilot).
11. I'm asking to write unit tests, if few projects then step by step.
12. Then pushing the code and then once copilot will add some comments to my review or human will do it, I'm asking to retrieve comments from github and ask: check what is worth implementing regarding if we do it like described in the current codebase. After I will get summary I'm asking to fix or give me the comment to respond.

So there is a lot of manual steps there as you can see. Also I do it like an artist, in random order and as I think at the moment. I would like to write an app to organize and automate my process.

This is my idea:
Write desktop app in C# that will be cross-platform (Windows, Mac, Linux, as an addition Android, iOS) which will automate my process of working on tasks and llms.

It should do the following steps:
1. Get information from jira by giving a ticket number. If user has no access to jira it can be pasted automatically.
2. Add catalogs from disks with info what it is and also add text/image content that will be linked in spec.md
3. We have predefined things to select (by default selected all) mentioned in my flow number 3. For example do not ask about easy things etc.
4. We can describe general context. For example in my current project I have: We have legacy app and rewriting to the new app, all we have must work same as in old app unless it is described differently in jira ticket. Also I'm typing this always so it can be stored and selected from the list.
5. At this point we should have ready to use spec.md and prompt generated to start using copilot. For example please implement locationxxx/spec.md, all images are in locationxxx/images etc.
6. At this point we can switch to copilot/claude and do the job. In our initial prompt we should include information for any changes or decisions, importan implementation notes please add entries in locationxxx/implementation.
7. Once feature is implemented then again in our software.
8. Code review and use skill if possible it should also give us prompt and ask to create review file in locationxxx/review-self
9. In next step our app should connect to github and get past comments and give us prompt with things to check locationxxx/review-against-past-comments
10. In next step we can review human/copilot comments for our review it can be stored in locationxxx/review-pr-comments 

This is more less, so it should have pane with the steps, but I do not need to move step by step, I can switch between steps.
Important info:
- we can use local ollama via microsoft agent framework or extensions.ai to fix some typos and do some small stuff
- we can use official attlasian/github packages to connect to github and jira
- we should edit spec and other md files in our software
- all changes can be versioned, there should be a special save button with save previous versions
- we can add next spec.md if we are doing modifications to specific feature already implemented
- we should have a tool to optimize context, maybe by using local llm
- every step can suggest best model to use
- I should be albe to organize by projects and inside the projects by features and inside features by tasks
- if I will do next task inside features it can get some knowledge by alanylizing other tasks in this features, feature itself and project. So for example general info for example, we are rewriting old app in this location etc. will be on project level

Automate as much as we can.

UI should be slick, clean, modern. Use avalonia. I have copilot-instructions/claude.md for other project here: C:\WIP\leontes\.github do not think that is useful for this but we should also write instructions similar for this project. But this one will be mvvm I guess. Our project should have nice layers, avalonia front, c# backend etc. but use global industry standards. Layers are necessary as later I will share some features as mcp server.

Ask about all unknowns. Do you have overall idea what I want to achieve. You can think outside the box, not only suggesting my workflow. Give be best experience in the world. By the way my project is named leontes. Do not be confused as my other project (AI agent) is leontes as well.