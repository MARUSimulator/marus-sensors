# Sensor usage

To use sensors with a data transmition adapter is neccessary to use gRPC integration scripts which enable communication with transmission tools.
To establish a remote connection subscribe the desired tool to a topic listed in the "Address" property of the sensor in Unity. 




## Ais

This sensor calculates and updates several parameters, including True Heading, SOG (Speed Over Ground), and COG (Course Over Ground) based on .

## GPS/GNSS



The primary purpose of this sensor is to measure geographic position, represented as latitude, longitude, and altitude. This is achieved by converting the sensor's local position in Unity to geographic coordinates.





## IMU
Imu sensor provides information on the vehicle's linear and angular velocity and orientation(heading).

To use this sensor make sure the vehicle model has a RigidBody component with mass.



## Lidar

This sensor uses raycasting to simulate a LiDAR system. It casts a number of horizontal and vertical rays to detect objects within its range. PointCloud data can be visualized in Unity using the PointCloud shader.



## Camera

Camera sensor creates a seperate view of the environment and can be displayed in the main viewport.

## Depth sensor


This sensor calculates the depth of an object relative to the water surface.

## DVL sensor

This sensor measures the ground velocity of the device it's attached to, which is the velocity vector relative to the seafloor. The sensor casts an array of range sensors to determine the distance from the object to the seafloor. 


## Sonar


This sensor generates 3D sonar images. It casts a number of acoustic rays (both horizontally and vertically) to map the underwater environment. The sensor produces both polar and cartesian 2D sonar images.

## Pose sensor

The sensor tracks the 3D position  and orientation of the object it is attached to. Orientation is represented as a quaternion, which is a standard way of encoding 3D rotations.



