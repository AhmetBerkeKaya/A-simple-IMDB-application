# A-simple-IMDB-application
The program contains information, including information and posters of popular movies. You can search the movie by some time period.
Yesilcam Film System Code Review
Project Overview
Yesilcam Film System is a Windows Forms application developed to manage and monitor Yeşilçam film data. It stores movie, actor, director and movie type data using the PostgreSQL database and provides access to users.

Key Features
Film Management

There are features to list, add, update and delete movies.
View movies associated with actors, directors, and movie genres.
Player Management

There are features to list, add, update and delete players.
Ability to list the movies the actors starred in.
Director Management

Features include listing, adding, updating and deleting directors.
Being able to list the films shot by the directors and the awards they received.
Film Genre Management

There are features to list, add, update and delete movie genres.


Connection Management

A connection to the PostgreSQL database was established using the NpgsqlConnection class.
Error situations were managed by checking the connection status.

Form Transactions

Lists of movies, actors, directors and movie genres are shown on the main form (Form1).
The visibility of a tag is changed at certain intervals using a timer.

Database Operations

Separate listing, addition, update and deletion operations were carried out for movie, actor, director and movie type data.
Validation checks were made during data entry.

Film Processing

Listing and updating of information associated with the actors, directors and genres of the films has been carried out.
DataGridView control is used to display the movie list.
Movie posters are stored in the folder called Posters.

Player Transactions

Listing and updating of the movies the actors starred in has been made.
DataGridView control is used to display the player list.

Director Transactions

The films made by the directors and the awards they received are listed.
DataGridView control is used to show the director list.

Film Type Operations

Listing, adding, updating and deleting movie genres have been done.
DataGridView control is used to display the list of movie genres.
