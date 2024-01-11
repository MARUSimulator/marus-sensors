# Sensor usage

To use sensors with a data transmition adapter is neccessary to use gRPC integration scripts which enable communication with transmission tools.

### ROS
ROS communications are enabled by RosConnection class providing a connection to ROS server

## Ais
Currently AIS sensor does not provide a ROS connection.

## GPS/GNSS

GPS/GNSS sensor supports connections with servers such as ROS. To integrate it inside a scene it is neccessary to attach add a component with GnssgRPC.cs (marus-sensors-grpc) script attached. This includes GnssSensor.cs script(marus-sensors) and Tf Streamer ROS script for communication. 

The primary purpose of this sensor is to measure geographic position, represented as latitude, longitude, and altitude. This is achieved by converting the sensor's local position in Unity to geographic coordinates.

Sensor data can be acquired through ROS server by connecting to a topic listed in the "Address" property of the sensor node. GnssSensor itself does not transmit data without a gRPC connection.

ROS Topic type: sensor_msgs/msg/NavSatFix 

## IMU

IMU sensor supports connections with servers such as ROS. To integrate it inside a scene it is neccessary to attach add a component with ImugRPC.cs (marus-sensors-grpc) script attached. This includes ImuSensor.cs script(marus-sensors) and Tf Streamer ROS script for communication. Make sure that the vehicle model has a RigidBody component with a mass.

Imu sensor provides information on the vehicle's linear and angular velocity and orientation(heading).

Sensor data can be acquired through ROS server by connecting to a topic listed in the "Address" property of the sensor node. ImuSensor itself does not transmit data without a gRPC connection.

ROS Topic type: sensor_msgs/msg/Imu

## Lidar
Lidar sensor supports connections with servers such as ROS. To integrate it inside a scene it is neccessary to attach add a component with RaycastLidargRPC.cs (marus-sensors-grpc) script attached. This includes RaycastLidarSensor.cs script(marus-sensors) and Tf Streamer ROS script for communication. 

Sensor data can be visualized in Unity with PointCoudShader which is included in the RaycastLidar.cs.
Data acquiring through ROS is established by a grpc adapter and marus-sensors-grpc scripts and is done by subscribing to a topic listed in the "Address" property of the sensor node.

ROS Topic type: sensor_msgs/msg/PointCloud2

## Camera

Camera sensor creates a seperate view of the environment and can be displayed in the main viewport. To establish remote connection use grpc-adapter script CameraSensorgRPC.cs and subscribe desired tool to a topic listed in the "Address" property of the camera sensor. 

ROS Topic type: sensor_msgs/msg/Image

## Depth sensor
Depth sensor supports connections with servers such as ROS. To integrate it inside a scene it is neccessary to attach add a component with DepthSensorgRPC.cs (marus-sensors-grpc) script attached. This includes DepthSensor.cs script(marus-sensors) and Tf Streamer ROS script for communication. 

Sensor data can be acquired through ROS server by connecting to a topic listed in the "Address" property of the sensor node. ImuSensor itself does not transmit data without a gRPC connection.


Topic type: geometry_msgs/msg/PoseWithCovarianceStamped


## DVL sensor

Depth sensor supports connections with servers such as ROS. To integrate it inside a scene it is neccessary to attach add a component with DvlSensorgRPC.cs (marus-sensors-grpc) script attached. This includes DvlSensor.cs script(marus-sensors) and Tf Streamer ROS script for communication. 

The primary function of the DVL sensor is to measure the ground velocity of the device it's attached to. This is achieved by calculating the difference in the device's position over time, which is then converted to velocity.

Sensor data can be acquired through ROS server by connecting to a topic listed in the "Address" property of the sensor node. 

ROS Topic type: geometry_msgs/msg/TwistWithCovarianceStamped


## Sonar

Sonar supports connections with remote servers such as ROS. To integrate it with a scene it is neccessary to attatch a component with Sonar3DgRPC.cs script.

The primary function of this sensor is to generate 3D sonar images. It casts a number of acoustic rays (both horizontally and vertically) to map the underwater environment. The sensor produces both polar and cartesian 2D sonar images.

## Pose sensor

    The sensor tracks the 3D position (position) and orientation (orientation) of the object it is attached to. Orientation is represented as a quaternion, which is a standard way of encoding 3D rotations.

Velocity Measurements:

    It measures the linear velocity (linearVelocity) and angular velocity (angularVelocity) of the object

