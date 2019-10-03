import React from 'react';
import { HubConnectionBuilder } from '@aspnet/signalr';
import {createGlobalStyle} from 'styled-components'
import {Row, Col, Container} from 'react-bootstrap'
import LiveTripTile from './components/LiveTripTile'
import SpeedAlertTile from './components/SpeedAlertTile'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const GlobalStyle = createGlobalStyle`
  html, body {
    background-color: #ffffff;
    height: 100%;
  }
`;

class App extends React.Component
{
  constructor(props) 
  {
      super(props);

      this.state = {
        odometry: {
          MeasuredSpeed: 0,
          SpeedLimit: 80
        }
      }
  }

  componentDidMount() {

    var hubConnectionRef = new HubConnectionBuilder()
      .withUrl(process.env.REACT_APP_HUB_URL)
      .build();

    hubConnectionRef.start().then(
      this.setState({
        hubConnection: hubConnectionRef
      })
    ).then(
      this.setupListeners(hubConnectionRef)
    )
  }

  setupListeners(hubConnection) {
    hubConnection.on('speedAlerts', (speedAlertInfoMsg) => {
      toast.warn("Your vehicle exeeded your speed alert limit.", {
        position: toast.POSITION.TOP_RIGHT
      })
    });
  }

  showMap() {
    if (this.state.hubConnection != null) {
      return <LiveTripTile hubConnection={this.state.hubConnection}/>
    }
    return <div>loading...</div>
  }

  render() {
    return (
      <React.Fragment>
        <ToastContainer />
        <GlobalStyle />
        
        <Container>
          <Row>
            <Col className="col-9">
              {this.showMap()}
            </Col>
            <Col className="col-3">
              <SpeedAlertTile />
            </Col>
          </Row>
        </Container>
      </React.Fragment>
    )
  }
}

export default App;
