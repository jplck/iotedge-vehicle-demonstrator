import React from 'react'
import styled from 'styled-components'
import {Card} from 'react-bootstrap'

class DeviceTelemetryTile extends React.Component 
{
    constructor(props) {
        super(props);
        this.state = {
            trip: {
                latitude: 52.10,
                longitude: 12.10,
                tripDistance: 0,
                tripTime: 0
            },
            odometry: {
                speedLimit: 80,
                measuredSpeed: 0
            },
            zoom: 16
        }
    }

    isVehicleSelected() {
        return this.props.selectedVehicle !== null && this.props.selectedVehicle !== undefined
    }

    validateDeviceContext(deviceId) {
        return this.isVehicleSelected() && deviceId === this.props.selectedVehicle.deviceId
    }

    componentDidMount()
    {
        const connection = this.props.hubConnection;
        if (connection === null) {
            return;
        }
        connection.on('vehicleLocation', (location) => {
            var loc = JSON.parse(location)
            if (!this.validateDeviceContext(loc.deviceId)) {
                return
            }
            this.setState({
                trip: loc
            })
        });

        connection.on('odometry', (odometry) => {
            var odo = JSON.parse(odometry)
            if (!this.validateDeviceContext(odo.deviceId)) {
                return
            }
            this.setState({
                odometry: odo
            })
        });
    }
    
    render() {
        var vehicleAvailable = this.props.selectedVehicle !== undefined && this.props.selectedVehicle !== null
        return (
            <Card>
                <Card.Body>
                    <Card.Title>Speed alert</Card.Title>
                    <Card.Subtitle>Setup your alert below.</Card.Subtitle>
                    <Card.Body>
                        <div>Selected vehicle: {vehicleAvailable ? this.props.selectedVehicle.name : "-"}</div>
                        <div>Current speed: <b>{this.state.odometry.measuredSpeed} km/h</b></div>
                        <div>Trip distance: <b>{this.state.trip.tripDistance}m</b></div>
                        <div>Trip time: <b>{this.state.trip.tripTime}s</b></div>
                        <div>Latitude: <b>{this.state.trip.latitude}</b></div>
                        <div>Longitude: <b>{this.state.trip.longitude}</b></div>
                    </Card.Body>
                </Card.Body>
            </Card>
        )
    }
}

export default DeviceTelemetryTile;