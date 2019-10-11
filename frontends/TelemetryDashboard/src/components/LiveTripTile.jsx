import React from 'react'
import {Map, Marker, TileLayer, Popup} from 'react-leaflet'
import styled from 'styled-components'

const MapContainer = styled.div`
    .leaflet-container {
        height:500px;
        width: 100%;
    }
    border: 1px solid #CACACA;
`;

const MapContainerFooter = styled.div`
    border-top: 1px solid #CACACA;
    padding: 5px;
`;

class LiveTripTile extends React.Component 
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
        const position = [this.state.trip.latitude, this.state.trip.longitude];
        var vehicleAvailable = this.props.selectedVehicle !== undefined && this.props.selectedVehicle !== null
        return (
            <MapContainer>
                <Map center={position} zoom={this.state.zoom}>
                    <TileLayer
                        attribution='&amp;copy <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    />
                    <Marker position={position}>
                        <Popup>
                            Popup for any custom information.
                        </Popup>
                    </Marker>
                </Map>
                <MapContainerFooter>
                    <div>Selected vehicle: {vehicleAvailable ? this.props.selectedVehicle.name : "-"}</div>
                    <div>Current speed: <b>{this.state.odometry.measuredSpeed} km/h</b></div>
                    <div>Trip distance: <b>{this.state.trip.tripDistance}m</b></div>
                    <div>Trip time: <b>{this.state.trip.tripTime}s</b></div>
                    <div>Latitude: <b>{this.state.trip.latitude}</b></div>
                    <div>Longitude: <b>{this.state.trip.longitude}</b></div>
                </MapContainerFooter>
            </MapContainer>
        )
    }
}

export default LiveTripTile;