# Adaptive Guidance and Validation in VR: Enhancing Procedural Performance Through Dynamic Visual Cues
*An Immersive VR Chemistry Laboratory for Procedural Training with Visual Guidance and Action Validation*

**Master's Thesis – University of Copenhagen, Department of Computer Science (2026)**

---

## Overview

This system is a virtual reality chemistry laboratory developed as part of my Master's thesis. The project investigates how different forms of support in VR training affect user performance, autonomy, and learning behavior during procedural laboratory tasks.

The system places users in an interactive chemistry laboratory where they complete multiple procedural organic chemistry experiments using virtual laboratory equipment such as bottles, beakers, reaction tubes, pipettes, spatulas, stirring rods, and water bath heaters.

The main research focus of the project is the comparison of different training conditions involving:

- **Visual guidance** – step-by-step visual cues that indicate what the user should interact with next;
- **Action validation** – immediate feedback on whether the performed action was correct or incorrect;
- **Combined support** – both guidance and validation enabled;
- **Independent execution** – no guidance or validation support.

The goal of the project is not only to evaluate whether guidance improves task performance, but also to explore whether users actively understand the procedure or become dependent on visual instructions.

---

## Research Context

Procedural training in VR is increasingly used in domains where real-world training can be expensive, unsafe, or difficult to repeat. Chemistry laboratories are a strong example of this, as they involve sequential procedures, specialized equipment, safety concerns, and a high need for accuracy.

This project explores how VR can be used to support procedural learning through:

- Immersive interaction with laboratory objects;
- Guided task execution;
- Immediate validation of user actions;
- Logging of task performance and mistakes;
- Comparison between different training support conditions;
- Post-task questionnaire evaluation.

The application was designed as both a working VR training prototype and an experimental platform for collecting user-study data.

---

## Core Features

### 1. Interactive VR Chemistry Laboratory

The system provides a virtual laboratory environment where users can interact with chemistry equipment in an immersive way.

Users can:

- Grab and move laboratory objects;
- Use a pipette to aspirate and dispense liquids;
- Use a spatula to transfer powders into beakers;
- Use a stirring rod to mix substances present in a beaker;
- Fill containers using the nozzle of bottles;
- Heat reaction tubes using an electric water bath heater;

The interactions are designed around VR controller input and object-based manipulation to simulate laboratory-style procedural work.

---

### 2. Procedural Experiment System

The application includes multiple organic chemistry experiments, each implemented as a structured procedure made up of ordered actions.

Each procedure tracks:

- The current expected action;
- The action performed by the user;
- Whether the action was correct or incorrect;
- The number of completed actions;
- The number of mistakes;
- Whether the procedure has been completed as a whole.

The system supports both linear procedures and procedures with multiple containers or sub-sequences, depending on the experiment.

---

### 3. Visual Guidance System

The visual guidance system helps users identify the next relevant object or interaction target in the procedure.

When guidance is enabled, the system can show visual cues for the objects needed in the current step. This helps users understand where to focus their attention and what action to perform next.

The guidance system includes:

- Initial guidance delay;
- Visual highlighting of relevant objects;
- Step-specific target selection;
- Guidance dismissal after the user interacts with the system in any way;
- Prevention of repeated guidance for dismissed actions;
- Automatic refresh when the current action changes.

This feature was designed to test whether visual support improves procedural performance and reduces confusion during VR training.

---

### 4. Action Validation System

The validation system checks whether the action performed by the user matches the expected step in the procedure.

When validation is enabled, the system provides feedback after each action:

- Correct actions through a green object outline of the container that the user has been working on;
- Incorrect actions through a red object outline of the container that the user has been working on.

The validation system allows the project to compare how immediate corrective feedback affects performance, confidence, and user behavior.

---

### 5. Experimental Conditions

The project supports four experimental conditions:

| Condition | Visual Guidance | Action Validation |
|---|---:|---:|
| Condition 1 | Enabled | Enabled |
| Condition 2 | Enabled | Disabled |
| Condition 3 | Disabled | Enabled |
| Condition 4 | Disabled | Disabled |

Each condition changes how much support the user receives while completing the procedure.

This structure allows the thesis to compare:

- Performance with and without guidance
- Performance with and without validation
- The combined effect of guidance and validation
- User independence when support is reduced or removed

---

### 6. Performance Logging

The project includes a logging system for collecting task performance data during each procedure.

For each participant and procedure, the system records:

- Participant ID;
- Timestamp;
- Procedure name;
- Whether guidance was enabled;
- Whether validation was enabled;
- Total procedure duration;
- Number of correct actions;
- Number of incorrect actions;
- Individual action durations;
- Expected action;
- Performed action;
- Whether each action was correct.

This data is exported in a structured format and can be used for later analysis in spreadsheets or statistical tools.

---

### 7. Participant-Based Study Support

The application was designed to support a user study as part of the thesis.

The study workflow includes:

- Participant identification;
- Multiple experiment conditions;
- Repeated procedural tasks;
- Automatic task completion detection;
- Transition screens between tutorial and experiment phases;
- Performance logging;
- Post-task questionnaire evaluation.

The questionnaire investigates aspects such as:

- Perceived guidance usefulness;
- Sense of autonomy;
- Dependence on visual guidance;
- Confidence;
- Workload;
- Perceived performance;
- User experience during the VR procedure.

---

## Architecture

This project is implemented in **Unity** using **C#**. The architecture is organized around modular manager and tracker scripts that separate experiment logic, interaction handling, guidance, validation, and logging.

---

### Procedure Tracking

Each experiment has a dedicated procedure tracker responsible for managing the current state of the procedure.

Procedure trackers handle:

- Current expected action;
- Registration of user actions;
- Correct and incorrect action counting;
- Action progression;
- Completion detection;
- Resetting the procedure state;
- Communication with validation and logging systems.

This makes each experiment self-contained while still following a shared procedural structure.

---

### Guidance Management

The guidance system is responsible for identifying and displaying the relevant guidance targets for the current action.

Main responsibilities include:

- Finding the next target objects;
- Showing visual cues after a delay;
- Clearing outdated guidance;
- Preventing dismissed guidance from reappearing too soon;
- Updating guidance when the current action changes.

The system is designed so that guidance can be enabled or disabled depending on the experimental condition.

---

### Validation Feedback

Validation components provide immediate visual feedback when the user performs an action.

The validation system is connected to the procedure tracker and receives information about:

- The expected action;
- The performed action;
- Whether the performed action is correct;
- Which object should display feedback.

This allows the system to visually communicate correctness without hard-coding validation behavior into every interactable object.

---

### Timing and Data Logging

A dedicated timing and logging component records procedure-level and action-level data.

The logger tracks:

- When a procedure starts;
- When each action starts and ends;
- How long each action takes;
- Total procedure duration;
- Correct and incorrect actions;
- Participant and condition information.

The resulting data can be exported and analyzed after the study.

---

### Experiment Setup and Transitions

The system includes setup switching and transition logic to move users between different phases of the experience.

This includes:

- Tutorial setup;
- Experiment setup;
- Fade transitions;
- Completion screens;
- Audio cues;
- Delayed display of end-of-procedure messages.

These elements help create a smoother user experience during the study.

---

## Tech Stack

**Engine & Language**

- Unity
- C#

**VR Development**

- Unity XR Toolkit
- VR controller-based interaction
- Object grabbing and manipulation
- World-space UI
- Canvas-based feedback screens

**Study & Data Collection**

- Custom procedure logging
- Participant ID handling
- Condition-based experiment configuration
- CSV/structured data export
- Questionnaire-based evaluation

**Development Tools**

- Unity Editor
- Visual Studio / Rider
- Git & GitHub

---

## Project Structure

The Unity project is organized into separate folders for assets, scenes, prefabs, scripts, XR configuration, and supporting resources.

```text
Assets/
│
├── Audios/
│
├── Images/
│
├── Materials/
│
├── Packages/
│
├── Prefabs/
│
├── Resources/
│
├── Samples/
│
├── Scenes/
│
├── Scripts/
│   ├── Actions/
│   ├── Audios/
│   ├── Beakers/
│   │   └── Liquids/
│   ├── Bottles/
│   ├── Clipboards/
│   ├── Containers/
│   ├── ElectricHeaters/
│   ├── Experiments/
│   ├── Guidance/
│   ├── Outlines/
│   ├── Participant/
│   ├── Pipettes/
│   │   ├── Inserters/
│   │   └── States/
│   ├── Powders/
│   ├── Procedures/
│   ├── ReactionTubes/
│   │   └── Liquids/
│   ├── Spatulas/
│   │   ├── Inserters/
│   │   └── States/
│   ├── StirringRods/
│   │   ├── Inserters/
│   │   └── States/
│   └── Validation/
│
├── XR/
└── XRI/
