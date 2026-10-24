# Rush Hour

A 2D transport management and simulation game built with **C# and Unity** for my A-Level Computer Science Non-Examined Assessment (NEA).

The game requires players to design and expand a public transport network across a growing fictional city, balancing passenger demand, journey times, maintenance costs, and available funding.

**Achievement:** 70/75 marks in my A-Level Computer Science NEA.

## Gameplay

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/19cf5840-b1d0-46da-b740-17d2d8fbc132" />

Players manage a city by building a public transport system that responds to the needs of its population.

- **Analyse the city:** View job density and population density across different districts to identify transport demand. Some districts in green are business districts (i.e. have many jobs located there so should be a priority to connect)
- **Build a transport network:** Draw connections between districts on the map to establish routes.
- **Deploy trains:** Add trains to the network to carry passengers between districts. Trains have different speeds, and capacities.
- **Simulate rush hour:** Run a simulation in which passengers attempt to travel using the available transport system.
- **Manage passenger journeys:** Passengers choose the fastest available route and use public transport. Beware that for longer journeys some cims may decide to use their car.
- **Balance costs and performance:** Account for ongoing maintenance costs while expanding the network, as soon as your balance goes negative, it's game over.
- **Grow the city:** Improve the public transport system's rating by increasing connections and ensuring full coverage, and continue expanding to meet demand.

The aim is to develop a transport network that efficiently serves the city's population while keeping journey times competitive with car travel.

<img width="1920" height="1080" alt="Screenshot (11)" src="https://github.com/user-attachments/assets/60a83028-0a71-4c0a-afef-810046bd95ff" />
<img width="1920" height="1080" alt="Screenshot (7)" src="https://github.com/user-attachments/assets/8f4ca3df-b1f0-4682-961c-d964b26a802a" />

## Technical Highlights

### Dijkstra's Shortest-Path Algorithm

Implemented Dijkstra's algorithm to determine the shortest paths through the transport network. This supports passenger route selection and helps the simulation determine how people travel between districts.

### Passenger and Transport Simulation

The game simulates passenger demand during rush hour, with trains moving around the map and collecting passengers. Passenger travel decisions depend on journey times, allowing the public transport network to compete with car travel.

### Demand-Based Network Design

Population and job density provide information about where transport connections are needed. Players use these visual indicators to plan and expand the network in response to demand. Drawing lines and allocating trains randomly will not help, and may lead to bankrupcy.

### Resource and Cost Management

Operating the transport network involves maintenance costs. Players must consider the cost of expanding and operating their system alongside its performance and public transport rating.

## Built With

- **C#** — Game logic, algorithms and simulation
- **Unity** — 2D game development and visualisation

## Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download)
- A compatible version of the Unity Editor
- Git

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/Oliver-TWyer/Rush-Hour.git
   ```

2. Open Unity Hub.
3. Select **Add project from disk** and select the cloned repository.
4. Open the project with the appropriate Unity Editor version.
5. Open the game's main scene and press **Play**.

## Project Background

Rush Hour was developed as my A-Level Computer Science NEA (AQA exam board). The project involved designing and implementing an interactive simulation that combines graph algorithms, transport routing, passenger behaviour and resource management.

It gave me practical experience applying algorithmic problem-solving to a larger software project using C# and Unity.

## Author

**Oliver Toraj-Wyer**

[GitHub Profile](https://github.com/Oliver-TWyer)

