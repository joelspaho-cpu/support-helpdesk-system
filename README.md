# Support Helpdesk System  
ASP.NET Core customer support ticketing system with role-based access, built to production-level standards

# Overview
This app was created as an alternative to standard support channels such as Chat or Phone, heavily inspired by personal work experience, where I was able to observe how a tech giant runs things, where time could be preserved such that Advisor workflows are efficient. I was heavily exposed to what is expected of these systems, how they are leveraged to assist customers and how things are monitored to maintain high security standards. All of this, contributed to this very project, which utilizes what I deemed as excellent features and things that needed improvement with the addition of missing features that make things run more efficiently.
# Tech Stack
- C# with ASP.NET Core framework is strongly typed, mature, performant, and critically, widely used in enterprise environments.
- PostgreSQL a free, open source, reliable, and excellently integrated in the .NET ecosystem. Industry standard for relational data.
- Razor Pages suits server-rendered apps with clear page-per-feature structure.
- Alpine.js provides dynamic UI interactions without pulling in a heavy framework like React. Keeps the project server-rendered which matches the stack.
- Railway/Azure provide cloud deployment.
# Architechture
Several architechtural decisions were made to ensure smooth operations across the board, below are a few examples to provide an idea:
- Queue
  - Tickets are categorised by status. A database query looks at how many active tickets the Advisor holds, those currently requiring Advisor action, and assigns to whoever has the least.
  - This principle similarly applies to Engineering and Level 2 when they interract with one another.
- Security 
  - Passwords are hashed for all actors, with staff also requiring mandatory 2-factor authentication in order to successfully sign in.
  - Tokenized based checks, if L1 attempts to access a L2 view, they will be prompted with an Access Denied screen and the incident will be logged.
  - Logs of various actions taken by staff and users are kept and fully reviewable by the relevant party. For instance, Managers have full access to Advisor logs, Admins have full access to Manager logs, Advisors have full access to user logs etc..
- Modularity and continuity
  - The system is expected to be sustainable and resistant to edge cases, recover from a crash, retain information on refresh or when navigating between pages etc
  - Disabling a feature does not crash the entire application, the system is expected to adapt accordingly if information is missing while avoiding bad states. In severe cases, an outright block is mandated.
# Roadmap
Ordered from most important to least important:
 ## Features for V1
  - Basic User functionality ( Log In or Sign Up, Verify Email, Review ongoing case(s), Create a ticket, Log Out )
  - Basic Advisor functionality ( Log In, 2FA verification, Ticket Manager, Review case(s), Change ticket statuses, Change work status, Log Out )
 ## Features for Beta
  - IP whitelisting such that internal systems are only visible and accessible by select users.
  - Implement an Admin role who overviews the entirety of the operations, has access to everything, can create Manager accounts and terminate them, can enable and disable features.
  - Implement a Manager who on top of being able to formally review cases (by providing feedback in how accurately the case was handled in accordance to documentation, visible to the advisor but not the User), can manage Advisors, review their tool usage, statuses, create Advisor accounts, terminate Advisor accounts, review statistics on a team level.
  - Implement a Level 2 Advisor who has more authority than a Level 1 Advisor, sees more knowledge base Articles, is able to create tickets and send them to Engineering (a department not visible to L1), create threads of issues that are reported by customers but not documented anywhere, able to mark customers as abusive such that they are no longer eligible for service etc...
  - Ability to review and upload attachments within a case, visible to the user and staff.
  - Ability for Advisors to look up tickets based on their ticket ID, the ticket reviewer is populated accordingly.
  - Ability for Advisors to escalate to different departments and roles within.
  - Ticket statuses should be previewed by a tiny colored circle to the left of the case title implying its importance, for example Dark RED - URGENT, Red - High priority, Yellow - Standard priority, Green - Low priority etc.., in parallel to this, on the right side we should have a timer which shows how long a ticket has gone on for without a reply.
  - A settings pane which appears upon clicking user profile on the top right, allows configuration of preferences (mainly for users) and adds the functionality of adding products via serial numbers, which obviously makes them selectable when creating tickets.
  - Advisors should be able to flag tickets to their judgement.
  - Advisors should be able to manipulate tickets and modify them as they see fit.
  - Customers should be notified via email when a change happens to their ticket, if its closed, escalated, replied to along with a quick link that brings them to the website for more details.
  - All customer facing roles should have a template list when dealing with tickets, these provide pre-written responses that the Advisor tailors to a specific situation, makes Advisor workflows considerably quicker.
  - Implement a knowledge base which features documents to aid Advisors in assisting Customers, internal procedures for various situations internal or external, each accessible by its unique code and visible depending on job role.
  ## Future plans
  - Implement a KPI dashboard where Advisors are able to preview their relevant business oriented KPI's such as handle time, customer satisfaction, escalation rates etc...
  - Email based survey system. After a ticket is marked as completed, an email is sent out to the user who evaluates the interraction with 3 options to choose from, Dissatisfied, Neutral, Satisfied. This is pushed to the advisor who handled the ticket, reflecting the change in their KPI dashboard.
  - Advisors should be able to set up phone appointments or store appointments if the situation calls for it.
  - Secure payment links, provided to customers depending on situation
  - Built-in translator
  - ML severity classificator right after ticket creation
  ## CI/CD
  - Build the Docker image in CI so a broken `Dockerfile` is caught on the PR rather than at deploy time.
  - Fail CI when the EF Core model has changes without a matching migration (`dotnet ef migrations has-pending-model-changes`).
  - Enforce code style with `dotnet format --verify-no-changes`.
  - Treat compiler warnings as errors so new warnings cannot accumulate.
  - Continuous deployment of `main` once a hosting target is chosen.
# Deployment
  - <link> - future cloud deployment once the software is ready
  
