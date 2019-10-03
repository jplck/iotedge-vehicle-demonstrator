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
                Latitude: 52.10,
                Longitude: 12.10
            },
            odometry: {
                SpeedLimit: 80,
                MeasuredSpeed: 0
            },
            zoom: 16
        }
    }

    componentDidMount()
    {
        const connection = this.props.hubConnection;
        if (connection === null) {
            return;
        }
        connection.on('vehicleLocation', (location) => {
            console.log(location.Latitude);
            this.setState({
                currentLoc: JSON.parse(location)
            })
        });

        connection.on('odometry', (odometry) => {
            this.setState({
                odometry: JSON.parse(odometry)
            })
        });
    }
    
    render() {
        const position = [this.state.currentLoc.Latitude, this.state.currentLoc.Longitude];
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
                    <div>Current speed: <b>{this.state.odometry.MeasuredSpeed} km/h</b></div>
                    <div>Latitude: <b>{this.state.currentLoc.Latitude}</b></div>
                    <div>Longitude: <b>{this.state.currentLoc.Longitude}</b></div>
                </MapContainerFooter>
            </MapContainer>
        )
    }
}

export default LiveTripTile;