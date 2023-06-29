# Patreons
Daniel Brvnišťan, David Roško

# FOMODog: Your Ultimate FOMO Buster! :dog:

Does the fear of missing out (FOMO) on exciting events shared in a Telegram chat give you sleepless nights? Or make you compulsively check your phone every few minutes? Well, say goodbye to your FOMO because FOMODog is here to rescue you! 

This nifty project monitors your Telegram chat with a friend and barks (not literally, of course) when it spots any events/fun happenings being shared. No more missing out on spontaneous road trips because you were busy watching cat videos, or an impromptu Tacos cook-off while you were neck-deep in unnecessary office meetings! 

But hold on, the fun doesn't stop here! Our FOMODog doesn't just alert you about events. It goes the extra mile (like your favorite Retriever) to replace simple, unexciting URLs with lively descriptions using mind-boggling AI resources. By transforming boring https links into exciting text, it ensures your Telegram chat stays as lively and enticing as a wicked game of fetch! 

Plus, if you're the kind who loves the drama of sending "42" or "I have FOMO", our FOMODog brings you an added layer of interactivity and humor to keep your chat excitement level always at its peak!

So, if you're ready to kick FOMO to the curb and sprinkle some magic onto your otherwise mundane chats, clone this project, replace the placeholders with your keys, and watch the FOMODog at work! 

Happy eventful chatting, folks! :)


This project includes several Docker, .NET Core, C# class files and is supposed to deploy a Telegram bot using Docker containerization. Follow the steps below to deploy the bot:

## Prerequisite:

Ensure that [Docker](https://www.docker.com/get-started) and [.NET Core](https://dotnet.microsoft.com/download) are installed on your system.

## Steps:

**Step 1**: Clone the repository to your local system. Using command prompt navigate to the folder where the Dockerfile and docker-compose files are located.

**Step 2**: Open docker-compose.yml file with a text editor. Replace the placeholders `{PUT_HERE_YOUR_....}` with your respective configuration values.
```
version: '3.7'
services:
  fomodog:
    restart: always
    image: smixers/fomodog:latest
    container_name: fomodog
    mem_limit: 128M
    mem_reservation: 128M
    environment:
      - Options__API_KEY={PUT_HERE_YOUR_CHATGPT_API_KEY}
      - Options__API_URL={PUT_HERE_YOUR_CHATGPT_API_URL}
      - Options__TELEGRAM_KEY={PUT_HERE_YOUR_TELEGRAM_BOT_TOKEN}
      - Options__PROMPT_PREFIX={PUT_HERE_PROMPT}
```
Remember to keep your `API_KEY`, `API_URL`, `TELEGRAM_KEY` and `PROMPT_PREFIX` confidential.

**Step 3**: Create a directory named 'src' in the same directory as the Dockerfile. Copy all your project files to this 'src' directory. These are the files that contain all the necessary code for your project.

**Step 4**: Now, build the Docker image. Run the following command in your terminal:

```bash
docker-compose up --build
```
Docker will build an image according to the Dockerfile specifications and start a container based on that image. The bot should now be running within that container.

## Notes for Running the Bot:

- If you make any changes to the source code or dockerfile, you will need to rebuild the docker image with the `docker-compose up --build` command.
- Make sure the bot token you are using for telegram is valid.
- If API requests are not going through, ensure that their respective server configurations are correct.
- Remember to check the logs for any possible runtime errors.
  
Once the setup is done correctly, your Telegram bot should be up and running. Enjoy interacting with your bot!



## How to Contribute:

This project is open-source and we welcome contributions from all enthusiasts. Whether you are a veteran developer or a beginner, there are several ways in which you can contribute.

1. **Bug Reporting**: If you find a bug, please open an issue in the GitHub repository, providing as much information as you can about the bug and how it occurred.

2. **Feature Requests**: If you have an idea for a new feature or an enhancement to an existing feature, please open a feature request issue.

3. **Code Contributions**: If you're an experienced developer or just starting out, you can review the open issues, fork the repository, and submit a Pull Request. All the code contributions are appreciated. After you send a Pull Request, maintainers will review your contribution and merge it if everything is correct.

4. **Documentation**: Documentation is as important as code. If you spot a place where you think the documentation can be updated or improved, go ahead and edit it!

This project follows a standard [fork and pull model](https://docs.github.com/en/github/collaborating-with-issues-and-pull-requests/about-pull-requests) for contributions via pull requests. 

## Support on Patreon:

If you find this project useful, you might want to consider supporting it. 

One way to support this project is through Patreon. 

By supporting this project on Patreon, you help ensure its ongoing development and maintenance. Your contribution helps cover the costs associated with hosting, domain name registration, and other expenses.

Head over to our [Patreon page](https://www.patreon.com/FOMODOG) and choose a tier that's right for you. Every bit helps, and we greatly appreciate your support!

Remember, open-source projects thrive on the support from their community. Every contribution and support is valuable. Thank you in advance for your support and contributions. Happy coding!
