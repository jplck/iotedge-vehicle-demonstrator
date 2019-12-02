import React from 'react';
import { HubConnectionBuilder } from '@aspnet/signalr';
import {createGlobalStyle} from 'styled-components'
import {Row, Col, Container} from 'react-bootstrap'
import LiveTripTile from './components/LiveTripTile'
import SpeedAlertTile from './components/SpeedAlertTile'
import DeviceTelemetryTile from './components/DeviceTelemetryTile'
import VehicleSelectorTile from './components/VehicleSelectorTile'
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const GlobalStyle = createGlobalStyle`
  html, body {
    background-color: #ffffff;
    height: 100%;
    margin-top: 20px;
  }
`;

class App extends React.Component
{
  constructor(props) 
  {
      super(props);

      this.state = {
        websocket: null,
        selectedVehicle: null
      }

      this.vehicleSelected = this.vehicleSelected.bind(this);
  }

  componentDidMount() {
    var hubConnectionRef = new HubConnectionBuilder()
      .withUrl(process.env.REACT_APP_HUB_URL, { accessTokenFactory: () => this.props.signalRToken })
      .build();

    hubConnectionRef.start().then(
      this.setState({
        websocket: hubConnectionRef
      })
    )
  }

  vehicleSelected(pairing) {
    this.setState({
      selectedVehicle: pairing
    })
  }

  render() {
    if (this.state.websocket !== null) {
      return (
          <React.Fragment>
            <ToastContainer />
            <GlobalStyle />
            <Container fluid>
              <Row>
                <Col className="col-5">
                  <Container>
                    <Row>
                      <Col className="col-6">
                        <VehicleSelectorTile onSelect={this.vehicleSelected} websocket={this.state.websocket}/>
                      </Col>
                      <Col className="col-6">
                        <DeviceTelemetryTile selectedVehicle={this.state.selectedVehicle} hubConnection={this.state.websocket}/>
                      </Col>
                    </Row>
                    <Row>
                      <Col className="col-6">
                          <SpeedAlertTile websocket={this.state.websocket}/>
                      </Col>
                    </Row>
                  </Container>
                </Col>
                <Col className="col-7">
                  <LiveTripTile subscriptionKey={process.env.REACT_APP_MAPS_KEY} selectedVehicle={this.state.selectedVehicle} hubConnection={this.state.websocket}/>
                </Col>
              </Row>
            </Container>
          </React.Fragment>
        )
      }
      return (
        <div>Loading...</div>
      )
  }
}

export default App;
