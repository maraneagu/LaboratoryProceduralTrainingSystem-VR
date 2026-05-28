# Adaptive Guidance and Validation in VR

*Enhancing Procedural Performance Through Dynamic Visual Cues in a VR Chemistry Laboratory*

**Master's Thesis – University of Copenhagen, Department of Computer Science (2026)**

---

## Overview

This repository contains the Unity project developed for my Master's thesis, **Adaptive Guidance and Validation in VR: Enhancing Procedural Performance Through Dynamic Visual Cues**.

The project is an immersive virtual reality chemistry laboratory designed for procedural training. Users complete organic chemistry procedures by interacting with virtual laboratory equipment, while the system can provide different forms of support through visual guidance and action validation.

The thesis investigates how these support mechanisms influence procedural performance, user autonomy, perceived usefulness, workload, and confidence during VR-based laboratory training.

The system was designed both as:

* a working VR chemistry training prototype;
* an experimental platform for comparing different training support conditions;
* a data-logging system for collecting procedural performance metrics during a user study.

---

## Virtual Laboratory Environment

The application places users inside a virtual chemistry laboratory where they can inspect, grab, move, and use laboratory objects through VR controller-based interaction.

The environment was designed to support procedural chemistry training tasks that are sequential, object-based, and accuracy-sensitive.

<p align="center">
  <img src="README%20Images/laboratory_environment.png" width="800" alt="Virtual chemistry laboratory environment">
</p>

Users interact with the environment using laboratory objects such as beakers, reaction tubes, bottles, pipettes, spatulas, stirring rods, powders, and heating equipment. The system supports both free object manipulation and procedure-specific action checking.

---

## Thesis Motivation

Procedural training is important in domains where tasks must be performed in a correct sequence and where mistakes can affect safety, accuracy, or learning outcomes. Chemistry laboratories are a strong example because procedures often involve specialized equipment, sequential steps, and actions that may be difficult or costly to repeat in physical settings.

Virtual reality offers a controlled and repeatable alternative for practicing these types of procedures. However, VR training systems still need to decide how much support to give users. Too little support can leave users confused, while too much guidance can make users dependent on visual instructions rather than actively learning the procedure.

This thesis explores this balance by comparing two forms of support:

* **visual guidance**, which helps users identify what to interact with next;
* **action validation**, which provides feedback on whether the performed action was correct.

The goal was not only to evaluate whether support improves performance, but also to understand how different forms of support affect user independence, confidence, perceived usefulness, and overall experience.

---

## Experimental Conditions

The project supports four training conditions, created by combining guidance and validation in different ways.

| Condition             | Visual Guidance | Action Validation | Description                                                          |
| --------------------- | --------------: | ----------------: | -------------------------------------------------------------------- |
| Guidance + Validation |         Enabled |           Enabled | Users receive both visual cues and correctness feedback.             |
| Guidance Only         |         Enabled |          Disabled | Users receive visual cues, but no correctness feedback.              |
| Validation Only       |        Disabled |           Enabled | Users receive correctness feedback, but no step-by-step visual cues. |
| No Support            |        Disabled |          Disabled | Users complete the procedure independently.                          |

This structure allowed the thesis to compare:

* performance with and without visual guidance;
* performance with and without validation feedback;
* the combined effect of both support mechanisms;
* how users behave when support is reduced or removed.

---

## Visual Guidance System

The visual guidance system highlights the next relevant object or interaction target in the current procedure.

When guidance is enabled, the system waits for a short delay before displaying cues. This was designed to give users an opportunity to act independently before receiving assistance. If the user interacts with the system before the guidance appears, the cue is dismissed for that action.

The guidance system includes:

* delayed guidance display;
* step-specific target highlighting;
* automatic updates when the expected action changes;
* prevention of repeated guidance for dismissed steps;
* support for procedures involving multiple containers or targets.

---

## Action Validation System

The action validation system checks whether the user performed the expected action for the current procedural step.

When validation is enabled, the system provides immediate visual feedback:

* **green outline** for correct actions;
* **red outline** for incorrect actions.

This feedback allows users to understand whether their action was successful without requiring constant step-by-step instruction.

---

## Implemented Chemistry Procedures

The system includes four organic chemistry procedures. Each procedure was implemented as an ordered sequence of actions involving different laboratory objects and interaction types.

### 1. Iodoform Test

<p align="center">
  <img src="README%20Images/experiment_iodoform_test.png" width="700" alt="Iodoform Test in VR">
</p>

### 2. Brady's Test

<p align="center">
  <img src="README%20Images/experiment_brady_test.png" width="700" alt="Brady's Test in VR">
</p>

### 3. Fehling's Test

<p align="center">
  <img src="README%20Images/experiment_fehling_test.png" width="700" alt="Fehling's Test in VR">
</p>

### 4. Benedict's Test

<p align="center">
  <img src="README%20Images/experiment_benedict_test.png" width="700" alt="Benedict's Test in VR">
</p>

Each procedure tracks the current expected action, the user action, correctness, mistakes, action duration, and total procedure completion time.

---

## Interaction Tools

The system includes nine main interaction tools used across the chemistry procedures.

| Tool               | Preview                                                                         | Description                                                     |
| ------------------ | ------------------------------------------------------------------------------- | --------------------------------------------------------------- |
| Beaker             | <img src="README%20Images/tool_beaker.png" width="160">             | Used as a container for liquids, powders, and mixed substances. |
| Reaction Tube      | <img src="README%20Images/tool_reaction_tube.png" width="160">      | Used for small-scale reactions and heating steps.               |
| Reaction Tube Rack | <img src="README%20Images/tool_reaction_tube_rack.png" width="160"> | Holds reaction tubes during procedures.                         |
| Pipette            | <img src="README%20Images/tool_pipette.png" width="160">            | Used to aspirate and dispense liquids.                          |
| Spatula            | <img src="README%20Images/tool_spatula.png" width="160">            | Used to transfer powder into containers.                        |
| Stirring Rod       | <img src="README%20Images/tool_stirring_rod.png" width="160">       | Used to mix liquids and powders inside containers.              |
| Powder             | <img src="README%20Images/tool_powder.png" width="160">             | Represents solid substances used in the procedures.             |
| Bottle / Nozzle    | <img src="README%20Images/tool_bottle_nozzle.png" width="160">      | Used to fill containers with liquid reagents.                   |
| Electric Heater    | <img src="README%20Images/tool_electric_heater.png" width="160">    | Used to heat reaction tubes during selected procedures.         |

---

## Study Workflow

The application was designed to support a participant-based user study.

The study flow includes:

1. participant identification;
2. VR tutorial and interaction familiarization;
3. procedure briefing before each experiment;
4. completion of four chemistry procedures under different support conditions;
5. automatic performance logging;
6. post-task questionnaire collection.

Before each experiment, users enter a briefing scene where they can review the procedure steps on a clipboard. After reviewing the steps, they begin the actual experiment and perform the procedure from memory, with support depending on the assigned condition.

---

## Data Collection

The system automatically logs procedural performance data during each experiment.

The logged data includes:

* participant ID;
* procedure name;
* support condition;
* guidance enabled/disabled;
* validation enabled/disabled;
* total procedure duration;
* number of correct actions;
* number of incorrect actions;
* action-level duration;
* expected action;
* performed action;
* action correctness.

This data was used for statistical analysis of task performance across the four experimental conditions.

---

## Questionnaire Evaluation

After each procedure, participants completed a post-task questionnaire.

The evaluation included measures related to:

* perceived usefulness of guidance;
* perceived usefulness of validation;
* autonomy;
* confidence;
* perceived performance;
* workload;
* user experience;
* dependence on visual guidance.

The questionnaire data complemented the logged performance data and helped evaluate not only whether users performed better, but also how they experienced the different support mechanisms.

---

## Main Thesis Contributions

This project contributes:

* a VR chemistry laboratory prototype for procedural training;
* an implementation of delayed, step-specific visual guidance;
* an implementation of immediate action validation feedback;
* four procedural organic chemistry experiments in VR;
* a condition-based experimental system for comparing guidance and validation;
* a logging system for action-level and procedure-level performance analysis;
* a user-study setup for evaluating performance, autonomy, workload, and perceived support.

The thesis findings suggest that validation feedback was especially valuable as a support mechanism because it helped users understand whether their actions were correct while still allowing them to act independently. Visual guidance was useful for directing attention and reducing confusion, but its role is most interesting when treated as adaptive support rather than continuous instruction.

---

## Architecture

The project is implemented in **Unity** using **C#**. The architecture is organized around modular systems for interaction, procedure tracking, guidance, validation, audio feedback, transitions, and logging.

### Procedure Tracking

Each experiment has a dedicated procedure tracker responsible for managing the current state of the procedure.

Procedure trackers handle:

* current expected action;
* user action registration;
* correct and incorrect action counting;
* action progression;
* completion detection;
* reset behavior;
* communication with validation and logging systems.

### Guidance Management

The guidance system identifies which objects should be highlighted for the current procedural step.

It handles:

* delayed guidance display;
* target selection;
* cue clearing;
* guidance dismissal;
* guidance refresh when the action changes;
* condition-based activation and deactivation.

### Validation Feedback

The validation system receives information from the procedure tracker and displays visual feedback based on whether the performed action matches the expected action.

It handles:

* correct action feedback;
* incorrect action feedback;
* target object outline selection;
* condition-based activation and deactivation.

### Timing and Logging

The logging system records both procedure-level and action-level data.

It tracks:

* procedure start and end time;
* action start and end time;
* total duration;
* action duration;
* correct and incorrect actions;
* participant ID;
* condition configuration.

---

## Tech Stack

**Engine and Language**

* Unity
* C#

**VR Development**

* Unity XR Interaction Toolkit
* Meta Quest 2
* VR controller-based interaction
* Object grabbing and manipulation
* World-space UI
* Visual outlining and highlighting
* Audio feedback

**Study and Data Collection**

* Custom procedure logging
* Condition-based experiment configuration
* Participant ID handling
* CSV/structured export
* Questionnaire-based evaluation

**Development Tools**

* Unity Editor
* Visual Studio / Rider
* Git
* GitHub

---

## Project Structure

```text
Thesis/
│
├── Assets/
│   ├── Audios/
│   ├── Images/
│   ├── Materials/
│   ├── Prefabs/
│   ├── Resources/
│   ├── Scenes/
│   ├── Scripts/
│   │   ├── Actions/
│   │   ├── Audios/
│   │   ├── Beakers/
│   │   ├── Bottles/
│   │   ├── Clipboards/
│   │   ├── Containers/
│   │   ├── ElectricHeaters/
│   │   ├── Experiments/
│   │   ├── Guidance/
│   │   ├── Outlines/
│   │   ├── Participant/
│   │   ├── Pipettes/
│   │   ├── Powders/
│   │   ├── Procedures/
│   │   ├── ReactionTubes/
│   │   ├── Spatulas/
│   │   ├── StirringRods/
│   │   └── Validation/
│   │
│   ├── XR/
│   └── XRI/
│
├── Packages/
└── ProjectSettings/
```

Large third-party imported asset packages and generated Unity folders are excluded from the repository to keep the GitHub project lightweight.

---

## Repository Notes

This repository excludes generated Unity folders and large imported third-party asset packages, including:

* `Library/`
* `Temp/`
* `Obj/`
* `Builds/`
* large imported asset package folders
* Unity sample folders
* `.unitypackage` installer files

These files are not necessary for reviewing the project structure, scripts, thesis implementation, or core system logic.

---

## How to Open the Project

1. Clone the repository.
2. Open Unity Hub.
3. Select **Add project from disk**.
4. Choose the `Thesis/` folder.
5. Open the project using the Unity version used for development.
6. Restore any excluded third-party assets if needed for the full visual environment.

---

## Author

**Mara Neagu**
Master's Thesis, Department of Computer Science
University of Copenhagen, 2026
