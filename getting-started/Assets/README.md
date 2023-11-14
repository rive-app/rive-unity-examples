![Discord badge](https://img.shields.io/discord/532365473602600965)
![Twitter handle](https://img.shields.io/twitter/follow/rive_app.svg?style=social&label=Follow)

# Rive Unity

![Rive hero image](https://github.com/rive-app/rive-bevy/assets/13705472/bfa78329-4652-4330-8f46-70c7f365e7bd)

A Unity runtime library for [Rive](https://rive.app).

> [!NOTE]  
> The Rive Unity runtime is currently in **Technical Review**. During this stage the API could change. While it's ready for testing and your feedback is highly valued during this phase, we advise exercising caution before considering it for production builds. Log an issue in our [issues](https://github.com/rive-app/rive-unity/issues) or reach out to us on [Discord](https://discord.com/invite/FGjmaTr) or through our [Support Channel](https://rive.atlassian.net/servicedesk/customer/portals).

## Table of contents

- ⭐️ [Rive Overview](#rive-overview)
- 🚀 [Getting Started](#getting-started)
- 👨‍💻 [Contributing](#contributing)
- ❓ [Issues](#issues)

## Rive Overview

[Rive](https://rive.app) is a real-time interactive design and animation tool that helps teams
create and run interactive animations anywhere. Designers and developers use our collaborative
editor to create motion graphics that respond to different states and user inputs. Our lightweight
open-source runtime libraries allow them to load their animations into apps, games, and websites.

🏡 [Homepage](https://rive.app/)

📘 [General help docs](https://help.rive.app/) · [Rive Unity docs](https://help.rive.app/game-runtimes/unity)

🛠 [Learning Rive](https://rive.app/learn-rive/)

👾 [Rive for Game UI](https://rive.app/game-ui)

## Getting Started

To quickly experiment with rive-unity, run one of our example projects: https://github.com/rive-app/rive-unity-examples

For additional guides, refer to the [rive-unity documentation](https://help.rive.app/game-runtimes/unity).

The rive-unity package can be added from GitHub and choosing a version tag.

Update the `manifest.json`:
```json
"app.rive.rive-unity": "git@github.com:rive-app/rive-unity.git?path=package#v0.1.10",
```

Alternatively add through the Unity package manager UI:
1. Open Window -> Package Manager
2. Choose Add package from git URL...
3. Add the URL with version tag, for example

```
git@github.com:rive-app/rive-unity.git?path=package#v0.1.10
```

Ensure you're using the [latest version](https://github.com/rive-app/rive-unity/tags).

### Awesome Rive

For even more examples and resources on using Rive at runtime or in other tools, checkout the [awesome-rive](https://github.com/rive-app/awesome-rive) repo.


### Rendering Support

The rive-unity runtime makes use of the [Rive Renderer](https://rive.app/renderer) and is up to date with that latest C++ runtime version of Rive.
- Metal on Mac and iOS
- DX11 on Windows
- OpenGL on Windows

## Contributing

All contributions are welcome!

## Issues

Have an issue with using the runtime, or want to suggest a feature/API to help make your development
life better? Log an issue in our [issues](https://github.com/rive-app/rive-unity/issues) tab! You
can also browse older issues and discussion threads there to see solutions that may have worked for
common problems.

### Known Issues

- Visual glitches on DX11 on Windows
