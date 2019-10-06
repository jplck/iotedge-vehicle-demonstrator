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
            currentLoc: {
                latitude: 52.10,
                longitude: 12.10
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

    componentDidMount()
    {
        const connection = this.props.hubConnection;
        if (connection === null) {
            return;
        }
        connection.on('vehicleLocation', (location) => {
            var loc = JSON.parse(location)
            if (!this.isVehicleSelected() || loc.deviceId !== this.props.selectedVehicle.deviceId) {
                return
            }
            this.setState({
                currentLoc: loc
            })
        });

        connection.on('odometry', (odometry) => {
            var odo = JSON.parse(odometry)
            if (!this.isVehicleSelected() || odo.deviceId !== this.props.selectedVehicle.deviceId) {
                return
            }
            this.setState({
                odometry: odo
            })
        });
    }
    
    render() {
        const position = [this.state.currentLoc.latitude, this.state.currentLoc.longitude];
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
                    <div>Latitude: <b>{this.state.currentLoc.latitude}</b></div>
                    <div>Longitude: <b>{this.state.currentLoc.longitude}</b></div>
                </MapContainerFooter>
            </MapContainer>
        )
    }
}

export default LiveTripTile;