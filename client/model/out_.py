from .header import BaseMessage

from .. import xml_

@xml_.XMLInclude
class CreateBuildingMessage(BaseMessage):
    """ Create building Message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'Position': False, 'Type': False,  'Angle': False}


@xml_.XMLInclude
class CreateNodeMessage(BaseMessage):
    """ Create node Message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'Position': False, 'Type': False}


@xml_.XMLInclude
class CreatePropMessage(BaseMessage):
    """ Create prop Message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'Position': False, 'Type': False, 'Angle': False}


@xml_.XMLInclude
class CreateSegmentMessage(BaseMessage):
    """ Create segment Message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {
            'net_options': False, 'control_point': False, 'auto_split': False,
            'start_postition': False,'end_postition': False,
            'start_node_id': False, 'end_node_id': False,
            'start_dir': False, 'end_dir': False
        }

@xml_.XMLInclude
class CreateTreeMessage(BaseMessage):
    """ Create tree message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'Position': False, 'prefab_name': False}


@xml_.XMLInclude
class DeleteObjectMessage(BaseMessage):
    """ Delete object message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'id': False, 'keep_nodes': False, 'type': False}

@xml_.XMLInclude
class GetObjectMessage(BaseMessage):
    """ Get object message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'id': False, 'idString': False, 'type': False}


@xml_.XMLInclude
class GetObjectsFromIndexMessage(BaseMessage):
    """ Get object message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'index': False, 'type': False}


@xml_.XMLInclude
class MoveMessage(BaseMessage):
    """ Move object message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {
            'id': False, 'type': False, 'position': False,
            'angle': False, 'is_angle_defined': False
        }


@xml_.XMLInclude
class RenderCircleMessage(BaseMessage):
    """ Render circle message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'position': False, 'radius': False, 'color': False}


@xml_.XMLInclude
class RenderVectorMessage(BaseMessage):
    """ Render vector message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {
            'vector': False, 'origin': False, 'color': False,
            'length': False, 'size': False
        }

@xml_.XMLInclude
class SetNaturalResourceMessage(BaseMessage):
    """ Get object message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'cell_id': False, 'value': False, 'type': False}
