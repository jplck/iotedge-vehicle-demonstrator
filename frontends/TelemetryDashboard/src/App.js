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
          SpeedLimit: 80,
        },
        websocket: null
      }
  }

  componentDidMount() {
    console.log(this.props.authToken)
    var hubConnectionRef = new HubConnectionBuilder()
      .withUrl(process.env.REACT_APP_HUB_URL, { accessTokenFactory: () => this.props.signalRToken })
      .build();

    hubConnectionRef.start().then(
      this.setState({
        websocket: hubConnectionRef
      })
    )
  }

  showMap() {
    if (this.state.websocket != null) {
      return <LiveTripTile hubConnection={this.state.websocket}/>
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
              <SpeedAlertTile websocket={this.state.websocket}/>
            </Col>
          </Row>
        </Container>
      </React.Fragment>
    )
  }
}

export default App;
