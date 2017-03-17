# HeadTrack
A c# library for tracking face and inferring head position

You can track face and calculate the position of your head using this project.

##Algorithm
The project is a C# version combining [headtrackr](https://github.com/auduno/headtrackr) and [face_detect_n_track](https://github.com/hrastnik/face_detect_n_track)
- the first JS library uses 'Viola-Jones detection' for head detecting and 'Camshift' for head tracking
- the second C++ library uses 'Haar cascades' for head detecting and some trick(like 'template matching') to speed up the detecting time(so it's pure detecting).


This project uses the built-in 'Haar cascades' for face detection and implemented 'Camshift' and the tricks in face_detect_n_track.


##Choose of algorithm
- if you have a good light source, use the camshift algorithm, it's a little faster.
- however, if the light condition isn't good(which is the common cases), use the second one(it's always robust).

##dependences
- please install "Opencv for C#": [Emgu](http://www.emgu.com/wiki/index.php/Main_Page)
