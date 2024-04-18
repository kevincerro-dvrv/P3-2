# CONCLUSION

## Client authority

The use of client authority has been an effective solution for our project, allowing everything to run smoothly. By updating the transform locally, even though it doesn't actually change anything, and requesting the server to perform movements without conducting additional checks on the server, such as restricting the position to the game space, we have significantly simplified the process. This implementation has eliminated the need for complex server-side validations, improving the efficiency and speed of communication between the client and server. Thanks to this strategy, we have achieved greater fluidity in the online gaming experience, reducing workload and enhancing system responsiveness.

## Server authority

Utilizing server authority, although slower compared to client authority, offers a crucial advantage in terms of security and reliability. While client authority empowers the client to make certain decisions independently, server authority prioritizes safety by ensuring that all critical decisions are validated server-side. This approach might introduce a slight delay due to the additional round-trip communication required between the client and server. However, the trade-off is justified by the enhanced security and reduced susceptibility to cheating or unauthorized modifications by clients.

It's worth noting that, during movement, clients may experience latency, which can be particularly noticeable when using server authority. However, we've discovered that disabling interpolation has significantly mitigated this issue. By prioritizing safety over speed and implementing strategic optimizations like disabling interpolation, server authority provides a solid foundation for a stable and fair multiplayer gaming environment, ultimately fostering a more enjoyable experience for all players involved.

## Server authority + rewind

Implementing server rewind in Unity has been a persistent headache, compounded by the countless hours spent troubleshooting issues related to network transforms. Despite extensive research efforts, the solution remained elusive, ultimately leading to the decision to abandon network transforms altogether. This decision was driven by the realization that without this drastic step, the functionality simply wouldn't work as intended. The process has been frustrating and time-consuming, highlighting the complexities inherent in multiplayer game development. However, by removing network transforms and pursuing alternative strategies, such as server rewind and network variables, the team aims to regain lost ground and forge a more stable, reliable multiplayer experience.