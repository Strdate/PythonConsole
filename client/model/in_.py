from .header import BaseMessage, InstanceData, InstanceDataBase

from .. import xml_


@xml_.XMLInclude
class BatchObjectMessage(BaseMessage):
    """ BatchObject Message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {
            'array': True, 'lastVisitedIndex': False,  'endOfStream': False
        }


@xml_.XMLInclude
class BuildingData(InstanceData):
    """ Building data """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        ret = super().attributes.copy()
        ret.update({'angle': False})
        return ret


@xml_.XMLInclude
class NetNodeData(InstanceData):
    """ Network node data """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        ret = super().attributes.copy()
        ret.update({
            'building_id': False, 'seg_count': False,
            'terrain_offset': False, '_cachedSegments': False
        })
        return ret


@xml_.XMLInclude
class NetPrefabData(InstanceDataBase):
    """ Network prefab data """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        ret = super().attributes.copy()
        ret.update({
            'width': False, 'is_overground': False,
            'is_underground': False, 'fw_vehicle_lane_count': False,
            'bw_vehicle_lane_count': False
        })
        return ret


@xml_.XMLInclude
class NetSegmentData(InstanceData):
    """ Network segment data """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        ret = super().attributes.copy()
        ret.update({
            'start_node_id': False, 'end_node_id': False,
            'start_dir': False, 'end_dir': False,
            'bezier': False, 'length': False, 'is_straight': False
        })
        return ret


@xml_.XMLInclude
class NetSegmentListMessage(BaseMessage):
    """ List of network segments """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        ret = super().attributes.copy()
        ret.update({'list': True})
        return ret


@xml_.XMLInclude
class PropData(InstanceData):
    """ Prop data """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        ret = super().attributes.copy()
        ret.update({'angle': False})
        return ret


@xml_.XMLInclude('RunScriptMessage')
class RunScriptMessage(BaseMessage):
    """ Run script message """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'script': False, 'clipboard': True, 'searchPaths': True}


@xml_.XMLInclude
class TreeData(InstanceData):
    """ Tree data """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return super().attributes
